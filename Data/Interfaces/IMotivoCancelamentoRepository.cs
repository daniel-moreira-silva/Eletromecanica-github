namespace Data.Interfaces;

public interface IMotivoCancelamentoRepository : IRepository<MotivoCancelamento, MotivoCancelamentoFilter>
{
    Task<List<MotivoCancelamento>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}