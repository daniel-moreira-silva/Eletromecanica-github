namespace Business.Interfaces;

public interface IServicoSolicitadoService
{
    Task<Guid> AddAsync(ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<ServicoSolicitado?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ListaPaginada<ServicoSolicitadoList>> PaginatedGetAsync(ServicoSolicitadoFilter filter, CancellationToken cancellationToken);
    Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, CancellationToken cancellationToken = default);
    Task<List<ServicoSolicitado>> GetAllAsync(CancellationToken cancellationToken);
}
