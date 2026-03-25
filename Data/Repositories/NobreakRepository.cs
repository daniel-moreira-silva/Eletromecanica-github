namespace Data.Repositories;

public class NobreakRepository(DbConnection connection) : INobreakRepository
{
    public async Task<bool> AddAsync(Nobreak nobreak, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Nobreak
            (
                EquipamentoId,
                PotenciaVA,
                AutonomiaMinutos
            )
            VALUES
            (
                @EquipamentoId,
                @PotenciaVA,
                @AutonomiaMinutos
            );
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, nobreak, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(Nobreak nobreak, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Nobreak
            SET
                PotenciaVA = @PotenciaVA,
                AutonomiaMinutos = @AutonomiaMinutos
            WHERE EquipamentoId = @EquipamentoId;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, nobreak, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Nobreak?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                EquipamentoId,
                PotenciaVA,
                AutonomiaMinutos
            FROM Nobreak
            WHERE EquipamentoId = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<Nobreak>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}
