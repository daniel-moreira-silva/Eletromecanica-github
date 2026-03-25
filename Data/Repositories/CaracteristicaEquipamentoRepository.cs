namespace Data.Repositories;

public class CaracteristicaEquipamentoRepository(DbConnection connection) : ICaracteristicaEquipamentoRepository
{
    public async Task<bool> AddAsync(List<CaracteristicaEquipamento> caracteristicas, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO CaracteristicaEquipamento
            (
                EquipamentoId,
                Nome,
                Valor
            )
            VALUES
            (
                @EquipamentoId,
                @Nome,
                @Valor
            );
        ";

        var result = await connection.ExecuteAsync(new CommandDefinition(sql, caracteristicas, transaction, cancellationToken: cancellationToken));
        return result > 0;
    }

    public async Task<bool> RemoveAllByEquipmentIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DELETE FROM CaracteristicaEquipamento WHERE EquipamentoId = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new {Id = equipamentoId }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<List<CaracteristicaEquipamento>> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                Id,
                EquipamentoId,
                Nome,
                Valor,
                DataCriacao
            FROM CaracteristicaEquipamento
            WHERE EquipamentoId = @EquipamentoId
            ORDER BY Nome;
        ";

        return (await connection.QueryAsync<CaracteristicaEquipamento>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken))).AsList();
    }
}
