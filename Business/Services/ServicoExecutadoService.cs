namespace Business.Services;

public class ServicoExecutadoService(IServicoExecutadoRepository repository) : IServicoExecutadoService
{
    public async Task<Guid> AddAsync(ServicoExecutado servicoExecutado, CancellationToken cancellationToken)
        => await repository.AddAsync(servicoExecutado, cancellationToken: cancellationToken);

    public async Task<List<ServicoExecutado>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(cancellationToken: cancellationToken);

    public async Task<ServicoExecutado?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken: cancellationToken);

    public async Task<ListaPaginada<ServicoExecutado>> PaginatedGetAsync(ServicoExecutadoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, cancellationToken: cancellationToken);

    public async Task<bool> UpdateAsync(ServicoExecutado servicoExecutado, CancellationToken cancellationToken)
        => await repository.UpdateAsync(servicoExecutado, cancellationToken: cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, cancellationToken: cancellationToken);

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateCodeIsDuplicatedAsync(codigo, id, cancellationToken: cancellationToken);
}