using Core.Models.FuncionarioAggregate;

namespace Business.Interfaces;

public interface ICargoService
{
    Task<Guid> AddAsync(Cargo cargo, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Cargo cargo, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Cargo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<Cargo>> PaginatedGetAsync(CargoFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<Cargo>> GetAllAsync(CancellationToken cancellationToken);
}