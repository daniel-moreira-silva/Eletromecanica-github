namespace Data.Interfaces;

public interface ICaracteristicaEquipamentoRepository
{
    Task<bool> AddAsync(List<CaracteristicaEquipamento> caracteristicas, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<CaracteristicaEquipamento>> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAllByEquipmentIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}