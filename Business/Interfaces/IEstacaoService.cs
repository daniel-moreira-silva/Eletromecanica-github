namespace Business.Interfaces;

public interface IEstacaoService
{
    Task<Guid> AddAsync(Estacao estacao, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Estacao estacao, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Estacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<Estacao>> PaginatedGetAsync(EstacaoFilter filter, CancellationToken cancellationToken);
    Task<List<TipoEstacao>> TipoEstacaoGetAllAsync(CancellationToken cancellationToken);
    Task<bool> ValidateNameIsDuplicatedAsync(string nome, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<Estacao>> GetAllAsync(CancellationToken cancellationToken);
}
