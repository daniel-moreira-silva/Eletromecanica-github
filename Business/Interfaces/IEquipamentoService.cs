namespace Business.Interfaces;

public interface IEquipamentoService
{
    Task<Guid?> AddAsync(EquipamentoCompletoDto equipamento, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(EquipamentoCompletoDto equipamento, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, bool ativo, CancellationToken cancellationToken);
    Task<Equipamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TipoEquipamento>> GetAllTiposEquipamentoAsync(CancellationToken cancellationToken);
    Task<ListaPaginada<Equipamento>> PaginatedGetAsync(EquipamentoFilter filter, CancellationToken cancellationToken);
    Task<List<Equipamento>> GetAllEquipmentsByEstacaoIdAsync(Guid estacaoId, bool? principal = null, CancellationToken cancellationToken = default);
    Task<EquipamentoCompletoDto?> GetCompleteEquipmentByIdAsync(Guid id, CancellationToken cancellationToken);
}
