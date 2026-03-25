namespace Data.Repositories;

public class MedidorVazaoRepository(DbConnection connection) : IMedidorVazaoRepository
{
    public async Task<bool> AddAsync(MedidorVazao medidorVazao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO MedidorVazao
            (
                EquipamentoId,
                Fabricante,
                ModeloConversor,
                ModeloSensor,
                Diametro,
                FatorK,
                EscalaMaxima
            )
            VALUES
            (
                @EquipamentoId,
                @Fabricante,
                @ModeloConversor,
                @ModeloSensor,
                @Diametro,
                @FatorK,
                @EscalaMaxima
            );
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, medidorVazao, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(MedidorVazao medidorVazao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE MedidorVazao
            SET
                EquipamentoId = @EquipamentoId,
                Fabricante = @Fabricante,
                ModeloConversor = @ModeloConversor,
                ModeloSensor = @ModeloSensor,
                Diametro = @Diametro,
                FatorK = @FatorK,
                EscalaMaxima = @EscalaMaxima
            WHERE EquipamentoId = @EquipamentoId;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, medidorVazao, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<MedidorVazao?> GetByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                EquipamentoId,
                Fabricante,
                ModeloConversor,
                ModeloSensor,
                Diametro,
                FatorK,
                EscalaMaxima
            FROM MedidorVazao
            WHERE EquipamentoId = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<MedidorVazao>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}
