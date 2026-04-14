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
        var numeroOs = await repository.GetNextNumberOSAynsc(transaction, cancellationToken);
        ordemServico.DataSolicitacao = DateTime.Now;
        ordemServico.StatusId = Guid.Parse(Constantes.OrdemServicoStatusSolicitada);
        ordemServico.Numero = numeroOs;
        ordemServico.Ano = DateTime.UtcNow.Year;
        ordemServico.SubOS = 0;
        ordemServico.Codigo = string.Concat(ordemServico.Numero, "/", ordemServico.Ano, "/", ordemServico.SubOS);
        ordemServico.IsAgendada = ordemServico.AgendamentoId != null;

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
        => await repository.UpdateAsync(ordemServico, cancellationToken: cancellationToken);

    public async Task<ListaPaginada<OrdemServico>> PaginatedGetAsync(OrdemServicoFilter filtro, CancellationToken cancellationToken)
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
}