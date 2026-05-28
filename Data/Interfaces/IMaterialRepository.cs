namespace Data.Interfaces;

public interface IMaterialRepository
{
    Task<Guid> AddAsync(Material material, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Material material, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Material?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<ListaPaginada<Material>> PaginatedGetAsync(MaterialFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<Material>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
