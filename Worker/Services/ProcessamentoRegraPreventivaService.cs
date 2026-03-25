namespace Worker.Services;

public class ProcessamentoRegraPreventivaService(
    IRegraPreventivaRepository repository,
    IOrdemServicoRepository ordemServicoRepository,
    IServicoSolicitadoRepository servicoSolicitadoRepository,
    IEstacaoRepository estacaoRepository,
    IRegraPreventivaService regraPreventivaService,
    IOrdemServicoService ordemServicoService,
    DbConnection connection,
    ILogger<ProcessamentoRegraPreventivaService> logger) : IProcessamentoRegraPreventivaService
{
    public async Task ProcessarRegraAsync(RegraPreventiva regra, CancellationToken cancellationToken)
    {
        if (regra.Id is null)
            return;

        bool regraDisponivelParaProcessamento = await regraPreventivaService.UpdateStatusEmProcessamentoAsync(regra.Id.Value, cancellationToken: cancellationToken);

        if (!regraDisponivelParaProcessamento)
        {
            logger.LogInformation("A regra preventiva {RegraId} já está sendo processada por outro worker. Pulando processamento.", regra.Id);
            return;
        }

        var today = DateTime.UtcNow.Date;

        var dataFiltro = RegraPreventivaUtils.CalcularProximaExecucao(today, -regra.Intervalo, regra.UnidadePeriodo);

        var servicosRegra = await servicoSolicitadoRepository.GetAllByRegraIdAsync(regra.Id.Value, cancellationToken: cancellationToken);

        var servicosDaRegra = servicosRegra.Select(x => x.Id!.Value).ToHashSet();

        if (servicosDaRegra.Count == 0)
        {
            logger.LogWarning("A regra preventiva {RegraId} não possui serviços vinculados. Nenhuma ação será executada.", regra.Id);
            await regraPreventivaService.UpdateStatusAguardandoProcessamentoAsync(regra.Id.Value, cancellationToken: cancellationToken);

            return;
        }

        var ordensServico = (await ordemServicoRepository.GetAllByEquipamentoIdAndDateAsync(regra.EquipamentoId, dataFiltro, cancellationToken: cancellationToken)).ToList();

        var servicosExecutados = await AnalisarExecucaoServicosAsync(ordensServico, servicosDaRegra, cancellationToken);

        var servicosPendentes = servicosDaRegra.Except(servicosExecutados.Keys).ToHashSet();

        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Sempre que houver serviços executados, divide por data.
            if (servicosExecutados.Count > 0)
            {
                var gruposPorData = servicosExecutados.GroupBy(x => x.Value.Date).OrderBy(x => x.Key).ToList();

                int controleRamificacao = 1;

                foreach (var grupo in gruposPorData)
                {
                    var dataExecucao = grupo.Key;
                    var servicosDoGrupo = grupo.Select(x => x.Key).ToHashSet();

                    var novaRegra = CriarNovaRegraParaServicosExecutados(regra, dataExecucao, controleRamificacao);

                    var idNovaRegra = await repository.AddAsync(novaRegra, transaction, cancellationToken);

                    await repository.AddRangeRegraPreventivaServicosSolicitadoAsync(idNovaRegra, servicosDoGrupo, transaction, cancellationToken);

                    controleRamificacao++;
                }

                logger.LogInformation(
                    "Regra {RegraId} foi processada. Pendentes mantidos: {Pendentes}. Novas regras criadas por data para serviços executados: {NovasRegras}.",
                    regra.Id,
                    servicosPendentes.Count,
                    gruposPorData.Count);
            }
            else
            {
                logger.LogInformation("Nenhum serviço executado encontrado para a regra {RegraId}. Regra mantida como está.", regra.Id);
            }

            // Depois da divisão, cria OS para os pendentes
            if (servicosPendentes.Count > 0)
            {
                bool osCriada = await CriarOsParaServicosPendentesAsync(regra, servicosPendentes, transaction, cancellationToken);

                if (!osCriada)
                    logger.LogWarning("Não foi possível criar OS preventiva para os serviços pendentes da regra {RegraId}. Serviços mantidos na regra para a próxima execução.", regra.Id);

                // Se ainda restaram pendentes na regra original, mantém a regra só com eles.
                regra.ServicosSolicitados = servicosPendentes
                    .Select(id => new RegraPreventivaServicoSolicitado
                    {
                        RegraPreventivaId = regra.Id.Value,
                        ServicoSolicitadoId = id
                    })
                    .ToList();
            }
            else
            {
                // Todos os serviços foram redistribuídos para novas regras.
                regra.Ativo = false;
                regra.ServicosSolicitados = [];
            }

            regra.UltimoProcessamento = today;
            await regraPreventivaService.UpdateAsync(regra, transaction, cancellationToken);
            await regraPreventivaService.UpdateStatusAguardandoProcessamentoAsync(regra.Id.Value, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Finalização do processamento da regra {RegraId}.", regra.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            await regraPreventivaService.UpdateStatusAguardandoProcessamentoAsync(regra.Id.Value, cancellationToken: cancellationToken);

            logger.LogError(ex, "Erro ao processar a divisão da regra preventiva {RegraId}.", regra.Id);
        }
    }

    private async Task<Dictionary<Guid, DateTime>> AnalisarExecucaoServicosAsync(
        IReadOnlyList<OrdemServico> ordensServico, HashSet<Guid> servicosDaRegra, CancellationToken cancellationToken)
    {
        var resultado = new Dictionary<Guid, DateTime>();

        var ordemIds = ordensServico.Select(x => x.Id!.Value).ToList();

        var todosServicos = await servicoSolicitadoRepository.GetAllByOrdensServicoIdsAsync(ordemIds, cancellationToken: cancellationToken);

        var servicosPorOrdem = todosServicos.GroupBy(x => x.OrdemServicoId).ToDictionary(g => g.Key, g => g.ToHashSet());

        foreach (var ordemServico in ordensServico)
        {
            if (!servicosPorOrdem.TryGetValue(ordemServico.Id!.Value, out var servicosOrdemIds))
                continue;

            var intersecao = servicosOrdemIds.Where(x => servicosDaRegra.Contains(x.ServicoSolicitadoId));

            foreach (var servico in intersecao)
            {
                var dataFinalizacao = ordemServico.DataFinalizacao?.Date ?? ordemServico.DataSolicitacao.Date;

                if (!resultado.TryGetValue(servico.ServicoSolicitadoId, out var ultimaData) || dataFinalizacao > ultimaData)
                    resultado[servico.ServicoSolicitadoId] = dataFinalizacao;
            }
        }

        return resultado;
    }

    private async Task<bool> CriarOsParaServicosPendentesAsync(
        RegraPreventiva regra,
        HashSet<Guid> servicosPendentes,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var estacao = await estacaoRepository.GetByEquipamentoId(regra.EquipamentoId, cancellationToken: cancellationToken);

        if (estacao?.Id is null)
        {
            logger.LogWarning("Estação não encontrada para o equipamento {EquipamentoId}. Não foi possível criar OS da regra {RegraId}.", regra.EquipamentoId, regra.Id);

            return false;
        }

        var novaOrdemServico = CriarOrdemServicoPreventiva(regra, estacao.Id.Value, servicosPendentes);

        var ordemCriada = await ordemServicoService.AddAsync(novaOrdemServico, transaction, cancellationToken);

        if (ordemCriada is null)
        {
            logger.LogWarning("A criação da OS preventiva retornou nulo para a regra {RegraId}.", regra.Id);

            return false;
        }

        logger.LogInformation("OS criada com sucesso para os serviços pendentes da regra {RegraId}. Quantidade: {Quantidade}.", regra.Id, servicosPendentes.Count);

        return true;
    }

    private static RegraPreventiva CriarNovaRegraParaServicosExecutados(RegraPreventiva regraOriginal, DateTime dataBaseExecucao, int ramificacaoIndex)
    {
        return new RegraPreventiva
        {
            EquipamentoId = regraOriginal.EquipamentoId,
            Nome = $"{regraOriginal.Nome} - ramificação {ramificacaoIndex}",
            Intervalo = regraOriginal.Intervalo,
            UnidadePeriodo = regraOriginal.UnidadePeriodo,
            DataInicio = dataBaseExecucao,
            UltimoProcessamento = dataBaseExecucao,
            ProximoProcessamento = RegraPreventivaUtils.CalcularProximaExecucao(dataBaseExecucao, regraOriginal.Intervalo, regraOriginal.UnidadePeriodo),
            Prioridade = regraOriginal.Prioridade,
            Ativo = true
        };
    }

    private static OrdemServico CriarOrdemServicoPreventiva(RegraPreventiva regra, Guid estacaoId, HashSet<Guid> servicoIds)
    {
        return new OrdemServico
        {
            EstacaoId = estacaoId,
            TipoOS = ETipoOS.Preventiva,
            Prioridade = regra.Prioridade,
            Observacao = $"Ordem de serviço gerada automaticamente pela regra preventiva: {regra.Nome}",
            ServicosSolicitados = servicoIds.Select(servicoId => new OrdemServicoServicoSolicitado { ServicoSolicitadoId = servicoId }).ToList(),
            Equipamentos = [new() { EquipamentoId = regra.EquipamentoId }]
        };
    }
}
