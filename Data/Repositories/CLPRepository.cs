namespace Data.Repositories;

public class CLPRepository(DbConnection connection) : ICLPRepository
{
    public async Task<bool> AddAsync(CLP clp, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO CLP
            (
                EquipamentoId,
                Marca,
                Firmware
            )
            VALUES
            (
                @EquipamentoId,
                @Marca,
                @Firmware
            );
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, clp, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(CLP clp, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE CLP
            SET
                Marca = @Marca,
                Firmware = @Firmware
            WHERE EquipamentoId = @EquipamentoId;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, clp, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<CLP?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                EquipamentoId,
                Marca,
                Firmware
            FROM CLP
            WHERE EquipamentoId = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<CLP>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}