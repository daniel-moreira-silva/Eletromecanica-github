using Core.Models.FuncionarioAggregate;

namespace Business.Interfaces;

public interface ISetorService
{
    Task<Guid> AddAsync(Setor setor, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Setor setor, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Setor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<Setor>> PaginatedGetAsync(SetorFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<Setor>> GetAllAsync(CancellationToken cancellationToken);
}