namespace Data.Interfaces;

public interface IEstacaoRepository : IRepository<Estacao, EstacaoFilter>
{
    Task<bool> ValidateNameIsDuplicatedAsync(string nome, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<TipoEstacao>> TipoEstacaoGetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<Estacao>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Estacao?> GetByEquipamentoId(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}