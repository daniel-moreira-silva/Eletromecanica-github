using Core.Models.FuncionarioAggregate;

namespace Business.Services;

public class TipoFuncionarioService(ITipoFuncionarioRepository repository) : ITipoFuncionarioService
{
    public async Task<Guid> AddAsync(TipoFuncionario tipoFuncionario, CancellationToken cancellationToken)
        => await repository.AddAsync(tipoFuncionario, null, cancellationToken);

    public async Task<List<TipoFuncionario>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<TipoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<TipoFuncionario>> PaginatedGetAsync(TipoFuncionarioFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(TipoFuncionario tipoFuncionario, CancellationToken cancellationToken)
        => await repository.UpdateAsync(tipoFuncionario, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateDescriptionIsDuplicatedAsync(descricao, id, null, cancellationToken);
}