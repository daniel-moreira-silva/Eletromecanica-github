using Core.Models.FuncionarioAggregate;

namespace Business.Interfaces;

public interface ITipoFuncionarioService
{
    Task<Guid> AddAsync(TipoFuncionario tipoFuncionario, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(TipoFuncionario tipoFuncionario, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<TipoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<TipoFuncionario>> PaginatedGetAsync(TipoFuncionarioFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<TipoFuncionario>> GetAllAsync(CancellationToken cancellationToken);
}