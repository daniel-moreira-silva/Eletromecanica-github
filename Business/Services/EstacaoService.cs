namespace Business.Services;

public class EstacaoService(IEstacaoRepository repository) : IEstacaoService
{
    public async Task<Guid> AddAsync(Estacao estacao, CancellationToken cancellationToken)
        => await repository.AddAsync(estacao, null, cancellationToken);

    public async Task<List<TipoEstacao>> TipoEstacaoGetAllAsync(CancellationToken cancellationToken)
        => await repository.TipoEstacaoGetAllAsync(null, cancellationToken);

    public async Task<Estacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, null, cancellationToken);

    public async Task<ListaPaginada<Estacao>> PaginatedGetAsync(EstacaoFilter filter, CancellationToken cancellationToken)
        => await repository.PaginatedGetAsync(filter, null, cancellationToken);

    public async Task<bool> UpdateAsync(Estacao estacao, CancellationToken cancellationToken)
        => await repository.UpdateAsync(estacao, null, cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken)
        => await repository.UpdateStatusAsync(id, ativo, null, cancellationToken);

    public async Task<bool> ValidateNameIsDuplicatedAsync(string nome, Guid? id = null, CancellationToken cancellationToken = default)
        => await repository.ValidateNameIsDuplicatedAsync(nome, id, null, cancellationToken);

    public async Task<List<Estacao>> GetAllAsync(CancellationToken cancellationToken)
        => await repository.GetAllAsync(null, cancellationToken);
}
