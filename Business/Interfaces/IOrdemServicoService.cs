namespace Business.Interfaces;

public interface IOrdemServicoService
{
    Task<OrdemServico?> AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken);
    Task<OrdemServico?> AddAsync(OrdemServico ordemServico, DbTransaction transaction, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken);
    Task<ListaPaginada<OrdemServicoList>> PaginatedGetAsync(OrdemServicoFilter filtro, CancellationToken cancellationToken);
    Task<IEnumerable<ListaCountOrdemServico>> ListaCountOrdemServicoAsync(OrdemServicoFilter filtro, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServico>> BuscarPorEnderecoAsync(string endereco, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoDto>> GetOrdemServicoNearByAsync(string lat, string lon, CancellationToken cancellationToken);
    Task<OrdemServicoDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Regiao>> GetAllRegioesOrdemServicoAsync(CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServico>> GetByAddressAsync(string search, CancellationToken cancellationToken);
    Task<bool> CancelarOrdemServicoAsync(Guid id, Guid motivoCancelamentoId, string observacao, CancellationToken cancellationToken);
    Task<bool> IniciarOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, CancellationToken cancellationToken);
    Task<bool> DespacharOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, DateTime dataDespachoProgramado, CancellationToken cancellationToken);
    Task<bool> DevolverOrdemServicoAsync(Guid ordemServicoId, string observacaoDevolucao, CancellationToken cancellationToken);
    Task<bool> AtualizarPrioridadeAsync(Guid ordemServicoId, EPrioridade prioridade, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoList>> GetSubOsListAsync(int numero, int ano, Guid excludeId, CancellationToken cancellationToken);
}
