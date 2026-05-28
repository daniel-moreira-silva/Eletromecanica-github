namespace Business.Interfaces;

public interface IMaterialService
{
    Task<Guid> AddAsync(Material material, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Material?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<Material>> PaginatedGetAsync(MaterialFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<Material>> GetAllAsync(CancellationToken cancellationToken);
}
