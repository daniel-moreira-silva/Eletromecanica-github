using Core.Models.FuncionarioAggregate;

namespace Business.Interfaces;

public interface IFuncionarioService
{
    Task<Guid> AddAsync(Funcionario funcionario, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<Funcionario>> PaginatedGetAsync(FuncionarioFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<Funcionario>> GetAllAsync(CancellationToken cancellationToken);
}