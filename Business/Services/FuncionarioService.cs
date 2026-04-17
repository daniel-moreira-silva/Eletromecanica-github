using Core.Models.FuncionarioAggregate;

namespace Business.Services;

public class FuncionarioService(IFuncionarioRepository repository) : IFuncionarioService
{
    public async Task<Guid> AddAsync(Funcionario funcionario, CancellationToken cancellationToken)
        => await repository.AddAsync(funcionario, null, cancellationToken);

    public async Task<List<Funcionario>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<Funcionario>> PaginatedGetAsync(FuncionarioFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken)
        => await repository.UpdateAsync(funcionario, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateCodeIsDuplicatedAsync(codigo, id, null, cancellationToken);
}