namespace Business.Services;

public class MaterialService(IMaterialRepository repository) : IMaterialService
{
    public async Task<Guid> AddAsync(Material material, CancellationToken cancellationToken)
        => await repository.AddAsync(material, null, cancellationToken);

    public async Task<List<Material>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);

    public async Task<Material?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<Material>> PaginatedGetAsync(MaterialFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(Material material, CancellationToken cancellationToken)
        => await repository.UpdateAsync(material, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateDescriptionIsDuplicatedAsync(descricao, id, null, cancellationToken);
}
