namespace Business.Interfaces;

public interface IRegraPreventivaService
{
    Task<Guid?> AddAsync(RegraPreventiva regra, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(RegraPreventiva regra, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(RegraPreventiva regra, DbTransaction transaction, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<RegraPreventiva>> GetAllPreventivasByEquipamentoIdAsync(Guid equipamentoId, CancellationToken cancellationToken);
    Task<bool> UpdateStatusEmProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAguardandoProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
