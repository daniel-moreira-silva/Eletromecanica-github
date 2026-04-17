namespace Business.Interfaces;

public interface IMotivoCancelamentoService
{
    Task<Guid> AddAsync(MotivoCancelamento motivo, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(MotivoCancelamento motivo, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<MotivoCancelamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<MotivoCancelamento>> PaginatedGetAsync(MotivoCancelamentoFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<MotivoCancelamento>> GetAllAsync(CancellationToken cancellationToken);
}