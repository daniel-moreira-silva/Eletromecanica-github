namespace Data.Interfaces;

public interface IRegraPreventivaRepository
{
    Task<Guid> AddAsync(RegraPreventiva regra, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(RegraPreventiva regra, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusEmProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAguardandoProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<RegraPreventiva>> GetAllByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<RegraPreventiva?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> AddRangeRegraPreventivaServicosSolicitadoAsync(Guid regraPreventivaId, IEnumerable<Guid> servicosSolicitadosIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAllRegraPreventivaServicosSolicitadoByRegraIdAsync(Guid regraPreventivaEquipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<RegraPreventivaServicoSolicitado>> GetAllRegraPreventivaServicosSolicitadoByRegraIdAsync(Guid regraPreventivaId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<RegraPreventiva>> GetAllByTodayAndAtivoAndAguardandoProcessamento(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
