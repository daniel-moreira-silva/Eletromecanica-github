

namespace Data.Interfaces;

public interface IEquipamentoRepository : IRepository<Equipamento, EquipamentoFilter>
{
    Task<List<Equipamento>> GetAllPrincipalEquipmentsByEstacaoIdAsync(Guid estacaoId, bool? principal = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<Equipamento>> GetAllComponentsByPrincipalEquipmentIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<TipoEquipamento>> GetAllTiposEquipamentoAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<Equipamento>?> GetAllEquipamentosByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
