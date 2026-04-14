namespace Data.Interfaces;

public interface IOrdemServicoRepository
{
    Task<Guid> AddAsync(OrdemServico ordemServico, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(OrdemServico os, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<OrdemServico?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<ListaPaginada<OrdemServico>> PaginatedGetAsync(OrdemServicoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, Guid statusId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<int> GetNextNumberOSAynsc(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServico>> GetByAddressAsync(string busca, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServico>> GetOrdemServicoNearByAsync(string lat, string lon, decimal raioKm, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServico>> GetAllByEquipamentoIdAndDateAsync(Guid id, DateTime date, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<Regiao>> GetAllRegioesOrdemServicoAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Guid> AddOrdemServicoServicoSolicitadoAsync(OrdemServicoServicoSolicitado ordemServicoServicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Guid> AddOrdemServicoEquipamentoAsync(OrdemServicoEquipamento ordemServicoEquipamento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ListaCountOrdemServico>> ListaCountOrdemServicoAsync(OrdemServicoFilter filtro, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}