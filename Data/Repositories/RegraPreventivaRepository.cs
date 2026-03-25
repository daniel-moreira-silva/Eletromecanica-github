namespace Data.Repositories;

public class RegraPreventivaRepository(DbConnection connection) : IRegraPreventivaRepository
{
    public async Task<Guid> AddAsync(RegraPreventiva regra, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DECLARE @Inserted TABLE (Id UNIQUEIDENTIFIER);

            INSERT INTO RegraPreventiva
            (
                EquipamentoId,
                Nome,
                UnidadePeriodo,
                StatusRegraPreventiva,
                Intervalo,
                DataInicio,
                ProximoProcessamento,
                UltimoProcessamento,
                Prioridade,
                Ativo
            )
            OUTPUT INSERTED.Id INTO @Inserted(Id)
            VALUES
            (
                @EquipamentoId,
                @Nome,
                @UnidadePeriodo,
                @StatusRegraPreventiva,
                @Intervalo,
                @DataInicio,
                @ProximoProcessamento,
                @UltimoProcessamento,
                @Prioridade,
                @Ativo
            );

            SELECT Id FROM @Inserted;
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, regra, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(RegraPreventiva regra, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE RegraPreventiva
            SET
                Nome = @Nome,
                UnidadePeriodo = @UnidadePeriodo,
                Intervalo = @Intervalo,
                DataInicio = @DataInicio,
                ProximoProcessamento = @ProximoProcessamento,
                UltimoProcessamento = @UltimoProcessamento,
                Prioridade = @Prioridade,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, regra, transaction, cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> UpdateStatusEmProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE RegraPreventiva
            SET StatusRegraPreventiva = 1
            WHERE Id = @RegraId AND StatusRegraPreventiva = 0
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { RegraId = regraId }, transaction, cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> UpdateStatusAguardandoProcessamentoAsync(Guid regraId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE RegraPreventiva
            SET StatusRegraPreventiva = 0
            WHERE Id = @RegraId AND StatusRegraPreventiva = 1
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { RegraId = regraId }, transaction, cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DELETE FROM RegraPreventiva
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<List<RegraPreventiva>> GetAllByEquipamentoIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM RegraPreventiva
            WHERE EquipamentoId = @EquipamentoId
            ORDER BY Nome, DataCriacao;
        ";

        var result = await connection.QueryAsync<RegraPreventiva>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));

        return result.AsList();
    }

    public async Task<RegraPreventiva?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM dbo.RegraPreventiva
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<RegraPreventiva>(new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> AddRangeRegraPreventivaServicosSolicitadoAsync(Guid regraPreventivaId, IEnumerable<Guid> servicosSolicitadosIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        if (!servicosSolicitadosIds.Any())
            return true;

        const string sql = @"
            INSERT INTO dbo.RegraPreventivaServicoSolicitado
            (
                RegraPreventivaId,
                ServicoSolicitadoId
            )
            VALUES
            (
                @RegraPreventivaId,
                @ServicoSolicitadoId
            );
        ";

        var parametros = servicosSolicitadosIds.Select(servicoId => new
        {
            RegraPreventivaId = regraPreventivaId,
            ServicoSolicitadoId = servicoId
        });

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, parametros, transaction, cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> RemoveAllRegraPreventivaServicosSolicitadoByRegraIdAsync(Guid regraPreventivaId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DELETE FROM RegraPreventivaServicoSolicitado
            WHERE RegraPreventivaId = @RegraPreventivaId;
        ";

        await connection.ExecuteAsync(new CommandDefinition(sql, new { RegraPreventivaId = regraPreventivaId }, transaction, cancellationToken: cancellationToken));

        return true;
    }

    public async Task<List<RegraPreventivaServicoSolicitado>> GetAllRegraPreventivaServicosSolicitadoByRegraIdAsync(Guid regraPreventivaId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                RPSS.RegraPreventivaId,
                RPSS.ServicoSolicitadoId,
                SS.Codigo,
                SS.Descricao
            FROM dbo.RegraPreventivaServicoSolicitado RPSS
            INNER JOIN dbo.ServicoSolicitado SS ON SS.Id = RPSS.ServicoSolicitadoId
            WHERE RPSS.RegraPreventivaId = @RegraPreventivaId
            ORDER BY SS.Descricao;
        ";

        var result = await connection.QueryAsync<RegraPreventivaServicoSolicitado>(new CommandDefinition(sql, new { RegraPreventivaId = regraPreventivaId }, transaction, cancellationToken: cancellationToken));

        return result.AsList();
    }

    public async Task<List<RegraPreventiva>> GetAllByTodayAndAtivoAndAguardandoProcessamento(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"
            SELECT *
            FROM RegraPreventiva
            WHERE Ativo = 1 AND CAST(ProximoProcessamento AS date) <= CAST(@Today AS date) AND StatusRegraPreventiva = 0
        ";

        var result = await connection.QueryAsync<RegraPreventiva>(new CommandDefinition(sql, new { Today = DateTime.UtcNow.Date }, transaction, cancellationToken: cancellationToken));

        return result.AsList();
    }
}