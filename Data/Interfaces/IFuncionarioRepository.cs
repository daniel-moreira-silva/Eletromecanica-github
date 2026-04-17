using Core.Models.FuncionarioAggregate;

namespace Data.Interfaces;

public interface IFuncionarioRepository : IRepository<Funcionario, FuncionarioFilter>
{
    Task<List<Funcionario>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}