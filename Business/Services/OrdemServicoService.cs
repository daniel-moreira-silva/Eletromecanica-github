namespace Business.Services;

public class OrdemServicoService(DbConnection connection,
    IConfiguration configuration,
    IOrdemServicoRepository repository,
    IEstacaoRepository estacaoRepository,
    IServicoSolicitadoRepository servicoSolicitadoRepository,
    IEquipamentoRepository equipamentoRepository,
    ILogger<OrdemServicoService> logger) : IOrdemServicoService
{
    public async Task<OrdemServico?> AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await CreateOrdemServicoAsync(ordemServico, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return ordemServico;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao adicionar ordem de serviço.");
            return null;
        }
    }

    public async Task<OrdemServico?> AddAsync(OrdemServico ordemServico, DbTransaction transaction, CancellationToken cancellationToken)
    {
        await CreateOrdemServicoAsync(ordemServico, transaction, cancellationToken);

        return ordemServico;
    }

    private async Task CreateOrdemServicoAsync(OrdemServico ordemServico, DbTransaction transaction, CancellationToken cancellationToken)
    {
        ordemServico.DataSolicitacao = DateTime.Now;
        ordemServico.StatusId = Guid.Parse(Constantes.OrdemServicoStatusSolicitada);

        if (ordemServico.OrdemServicoPaiId.HasValue)
        {
            var pai = await repository.GetByIdAsync(ordemServico.OrdemServicoPaiId.Value, transaction, cancellationToken)
                ?? throw new InvalidOperationException("Ordem de serviço pai não encontrada.");

            ordemServico.Numero = pai.Numero;
            ordemServico.Ano = pai.Ano;
            ordemServico.SubOS = await repository.GetNextSubOSAsync(pai.Numero!.Value, pai.Ano!.Value, transaction, cancellationToken);
        }
        else
        {
            ordemServico.Numero = await repository.GetNextNumberOSAynsc(transaction, cancellationToken);
            ordemServico.Ano = DateTime.UtcNow.Year;
            ordemServico.SubOS = 0;
        }

        ordemServico.Codigo = string.Concat(ordemServico.Numero, "/", ordemServico.Ano, "/", ordemServico.SubOS);
        ordemServico.IsAgendada = ordemServico.AgendamentoId != null;

        // Define a prioridade da ordem de serviço com base na prioridade dos serviços solicitados
        var servicosSolicitados = await servicoSolicitadoRepository.GetAllByIdListAsync(ordemServico.ServicosSolicitados?.Select(s => s.ServicoSolicitadoId).ToList() ?? [], transaction, cancellationToken);

        if (servicosSolicitados.Count > 0)
            ordemServico.Prioridade = servicosSolicitados.Max(s => s.Prioridade);

        var ordemServicoId = await repository.AddAsync(ordemServico, transaction, cancellationToken);

        foreach (var servico in ordemServico.ServicosSolicitados ?? [])
        {
            servico.OrdemServicoId = ordemServicoId;
            _ = await repository.AddOrdemServicoServicoSolicitadoAsync(servico, transaction, cancellationToken);
        }

        foreach (var ordemServicoEquipamento in ordemServico.Equipamentos ?? [])
        {
            ordemServicoEquipamento.OrdemServicoId = ordemServicoId;
            _ = await repository.AddOrdemServicoEquipamentoAsync(ordemServicoEquipamento, transaction, cancellationToken);
        }
    }

    public async Task<bool> UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!await repository.UpdateAsync(ordemServico, transaction, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await repository.DeleteServicosSolicitadosByOrdemServicoAsync(ordemServico.Id!.Value, transaction, cancellationToken);
            await repository.DeleteEquipamentosByOrdemServicoAsync(ordemServico.Id!.Value, transaction, cancellationToken);

            foreach (var servico in ordemServico.ServicosSolicitados ?? [])
            {
                servico.OrdemServicoId = ordemServico.Id.Value;
                await repository.AddOrdemServicoServicoSolicitadoAsync(servico, transaction, cancellationToken);
            }

            foreach (var equip in ordemServico.Equipamentos ?? [])
            {
                equip.OrdemServicoId = ordemServico.Id;
                await repository.AddOrdemServicoEquipamentoAsync(equip, transaction, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao atualizar ordem de serviço.");
            return false;
        }
    }

    public async Task<ListaPaginada<OrdemServicoList>> PaginatedGetAsync(OrdemServicoFilter filtro, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filtro, cancellationToken: cancellationToken);

    public async Task<IEnumerable<OrdemServico>> BuscarPorEnderecoAsync(string endereco, CancellationToken cancellationToken)
        => await repository.GetByAddressAsync(endereco, cancellationToken: cancellationToken);

    public async Task<IEnumerable<OrdemServicoDto>> GetOrdemServicoNearByAsync(string lat, string lon, CancellationToken cancellationToken)
    {
        _ = decimal.TryParse(configuration["BurcaOrdensServicoProximas:RaioKm"], out var raioKm);

        var result = (await repository.GetOrdemServicoNearByAsync(lat, lon, raioKm,  cancellationToken: cancellationToken)).Select(x => (OrdemServicoDto)x!).ToList();

        foreach (var ordemServico in result)
            ordemServico.ServicosSolicitados = await servicoSolicitadoRepository.GetAllByOrdemServicoAsync(ordemServico.Id, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<OrdemServicoDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        OrdemServicoDto? ordemServico = await repository.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (ordemServico == null)
            return null;

        ordemServico.Estacao = await estacaoRepository.GetByIdAsync(ordemServico.EstacaoId, cancellationToken: cancellationToken);
        ordemServico.ServicosSolicitados = await servicoSolicitadoRepository.GetAllByOrdemServicoAsync(ordemServico.Id, cancellationToken: cancellationToken);
        ordemServico.Equipamentos = await equipamentoRepository.GetAllEquipamentosByOrdemServicoAsync(ordemServico.Id, cancellationToken: cancellationToken);

        return ordemServico;
    }

    public async Task<List<Regiao>> GetAllRegioesOrdemServicoAsync(CancellationToken cancellationToken)
        => await repository.GetAllRegioesOrdemServicoAsync(cancellationToken: cancellationToken);

    public async Task<IEnumerable<OrdemServico>> GetByAddressAsync(string search, CancellationToken cancellationToken)
        => await repository.GetByAddressAsync(search, cancellationToken: cancellationToken);

    public async Task<IEnumerable<ListaCountOrdemServico>> ListaCountOrdemServicoAsync(OrdemServicoFilter filtro, CancellationToken cancellationToken)
        => await repository.ListaCountOrdemServicoAsync(filtro, cancellationToken: cancellationToken);

    public async Task<bool> CancelarOrdemServicoAsync(Guid ordemServicoId, Guid motivoCancelamentoId, string observacao, CancellationToken cancellationToken)
        => await repository.CancelarOrdemServicoAsync(ordemServicoId, motivoCancelamentoId, observacao, cancellationToken: cancellationToken);

    public async Task<bool> IniciarOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, CancellationToken cancellationToken)
        => await repository.IniciarOrdemServicoAsync(ordemServicoId, funcionarioId, cancellationToken: cancellationToken);

    public async Task<bool> DespacharOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, DateTime dataDespachoProgramado, CancellationToken cancellationToken)
        => await repository.DespacharOrdemServicoAsync(ordemServicoId, funcionarioId, dataDespachoProgramado, cancellationToken: cancellationToken);

    public async Task<bool> DevolverOrdemServicoAsync(Guid ordemServicoId, string observacaoDevolucao, CancellationToken cancellationToken)
        => await repository.DevolverOrdemServicoAsync(ordemServicoId, observacaoDevolucao, cancellationToken: cancellationToken);

    public async Task<bool> AtualizarPrioridadeAsync(Guid ordemServicoId, EPrioridade prioridade, CancellationToken cancellationToken)
        => await repository.AtualizarPrioridadeAsync(ordemServicoId, prioridade, cancellationToken: cancellationToken);

    public async Task<IEnumerable<OrdemServicoList>> GetSubOsListAsync(int numero, int ano, Guid excludeId, CancellationToken cancellationToken)
        => await repository.GetSubOsListAsync(numero, ano, excludeId, cancellationToken: cancellationToken);
}