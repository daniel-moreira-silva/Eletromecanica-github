using Core.Models.FuncionarioAggregate;

namespace Business.Services;

public class SetorService(ISetorRepository repository) : ISetorService
{
    public async Task<Guid> AddAsync(Setor setor, CancellationToken cancellationToken)
        => await repository.AddAsync(setor, null, cancellationToken);

    public async Task<List<Setor>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<Setor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<Setor>> PaginatedGetAsync(SetorFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(Setor setor, CancellationToken cancellationToken)
        => await repository.UpdateAsync(setor, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateDescriptionIsDuplicatedAsync(descricao, id, null, cancellationToken);
}