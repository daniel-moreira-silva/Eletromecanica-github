using Core.Models.FuncionarioAggregate;

namespace Business.Services;

public class CargoService(ICargoRepository repository) : ICargoService
{
    public async Task<Guid> AddAsync(Cargo cargo, CancellationToken cancellationToken)
        => await repository.AddAsync(cargo, null, cancellationToken);

    public async Task<List<Cargo>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<Cargo?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<Cargo>> PaginatedGetAsync(CargoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(Cargo cargo, CancellationToken cancellationToken)
        => await repository.UpdateAsync(cargo, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateDescriptionIsDuplicatedAsync(descricao, id, null, cancellationToken);
}