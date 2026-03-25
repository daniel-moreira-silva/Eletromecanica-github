namespace Business.Services;

public class ServicoSolicitadoService(IServicoSolicitadoRepository repository) : IServicoSolicitadoService
{
    public async Task<Guid> AddAsync(ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken)
        => await repository.AddAsync(servicoSolicitado, null, cancellationToken);

    public async Task<List<ServicoSolicitado>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<ServicoSolicitado?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<ServicoSolicitado>> PaginatedGetAsync(ServicoSolicitadoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken)
        => await repository.UpdateAsync(servicoSolicitado, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateCodeIsDuplicatedAsync(codigo, id, null, cancellationToken);
}
