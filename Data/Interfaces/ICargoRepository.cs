using Core.Models.FuncionarioAggregate;

namespace Data.Interfaces;

public interface ICargoRepository : IRepository<Cargo, CargoFilter>
{
    Task<List<Cargo>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}