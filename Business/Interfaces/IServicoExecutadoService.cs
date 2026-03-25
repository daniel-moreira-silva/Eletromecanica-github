namespace Business.Interfaces;

public interface IServicoExecutadoService
{
    Task<Guid> AddAsync(ServicoExecutado servicoExecutado, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(ServicoExecutado servicoExecutado, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<ServicoExecutado?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<ServicoExecutado>> PaginatedGetAsync(ServicoExecutadoFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<ServicoExecutado>> GetAllAsync(CancellationToken cancellationToken);
}
