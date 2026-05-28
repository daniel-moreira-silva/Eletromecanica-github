namespace Data.Interfaces;

public interface IServicoSolicitadoRepository
{
    Task<Guid> AddAsync(ServicoSolicitado servicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ServicoSolicitado servicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<ServicoSolicitado?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<ListaPaginada<ServicoSolicitadoList>> PaginatedGetAsync(ServicoSolicitadoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServicoServicoSolicitado>> GetAllByOrdensServicoIdsAsync(List<Guid> ordensServicoIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByRegraIdAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByIdListAsync(List<Guid> ids, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
