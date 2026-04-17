using Core.Models.FuncionarioAggregate;

namespace Data.Interfaces;

public interface ISetorRepository : IRepository<Setor, SetorFilter>
{
    Task<List<Setor>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}