namespace Data.Interfaces;

public interface IServicoSolicitadoRepository : IRepository<ServicoSolicitado, ServicoSolicitadoFilter>
{
    Task<List<ServicoSolicitado>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServicoServicoSolicitado>> GetAllByOrdensServicoIdsAsync(List<Guid> ordensServicoIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByRegraIdAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllByIdListAsync(List<Guid> ids, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
