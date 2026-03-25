namespace Core.Interfaces.Repositories;

public interface IRepository<T1, T2>
{
    Task<Guid> AddAsync(T1 value, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T1 value, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<T1?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<ListaPaginada<T1>> PaginatedGetAsync(T2 filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}