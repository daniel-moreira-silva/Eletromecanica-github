using Core.Models.FuncionarioAggregate;

namespace Data.Interfaces;

public interface ITipoFuncionarioRepository : IRepository<TipoFuncionario, TipoFuncionarioFilter>
{
    Task<List<TipoFuncionario>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}