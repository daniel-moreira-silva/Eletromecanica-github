namespace Data.Repositories;

public class BombaRepository(DbConnection connection) : IBombaRepository
{
    public async Task<bool> AddAsync(Bomba bomba, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Bomba
            (
                EquipamentoId,
                Vazao,
                AlturaManometrica,
                Potencia
            )
            VALUES
            (
                @EquipamentoId,
                @Vazao,
                @AlturaManometrica,
                @Potencia
            );
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, bomba, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(Bomba bomba, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Bomba
            SET
                Vazao = @Vazao,
                AlturaManometrica = @AlturaManometrica,
                Potencia = @Potencia
            WHERE EquipamentoId = @EquipamentoId;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, bomba, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Bomba?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                EquipamentoId,
                Vazao,
                AlturaManometrica,
                Potencia
            FROM Bomba
            WHERE EquipamentoId = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<Bomba>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}