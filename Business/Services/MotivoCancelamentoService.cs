namespace Business.Services;

public class MotivoCancelamentoService(IMotivoCancelamentoRepository repository) : IMotivoCancelamentoService
{
    public async Task<Guid> AddAsync(MotivoCancelamento motivo, CancellationToken cancellationToken)
        => await repository.AddAsync(motivo, cancellationToken: cancellationToken);

    public async Task<List<MotivoCancelamento>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(cancellationToken: cancellationToken);

    public async Task<MotivoCancelamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken: cancellationToken);

    public async Task<ListaPaginada<MotivoCancelamento>> PaginatedGetAsync(MotivoCancelamentoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, cancellationToken: cancellationToken);

    public async Task<bool> UpdateAsync(MotivoCancelamento motivo, CancellationToken cancellationToken)
        => await repository.UpdateAsync(motivo, cancellationToken: cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, cancellationToken: cancellationToken);

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateCodeIsDuplicatedAsync(codigo, id, cancellationToken: cancellationToken);
}