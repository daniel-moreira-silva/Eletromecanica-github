namespace Core.Interfaces.Repositories;

public interface IComplementoEquipamentoRepository<T>
{
    Task<bool> AddAsync(T complement, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T complement, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<T?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}