namespace Data.Interfaces;

public interface IServicoExecutadoRepository : IRepository<ServicoExecutado, ServicoExecutadoFilter>
{
    Task<List<ServicoExecutado>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<ServicoExecutado>> GetAllByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
