namespace Data.Repositories;

public class MotorRepository(DbConnection connection) : IMotorRepository
{
    public async Task<bool> AddAsync(Motor motor, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Motor
            (
                EquipamentoId,
                Potencia,
                Tensao,
                RPM
            )
            VALUES
            (
                @EquipamentoId,
                @Potencia,
                @Tensao,
                @RPM
            );
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, motor, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(Motor motor, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Motor
            SET
                Potencia = @Potencia,
                Tensao = @Tensao,
                RPM = @RPM
            WHERE EquipamentoId = @EquipamentoId;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, motor, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Motor?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                EquipamentoId,
                Potencia,
                Tensao,
                RPM
            FROM Motor
            WHERE EquipamentoId = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<Motor>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}