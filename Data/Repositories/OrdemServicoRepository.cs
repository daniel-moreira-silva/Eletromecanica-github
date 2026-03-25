namespace Data.Repositories;

public class OrdemServicoRepository(DbConnection connection) : IOrdemServicoRepository
{
    public async Task<Guid> AddAsync(OrdemServico ordemServico, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO OrdemServico
            (
                Codigo,
                Numero,
                SubOS,
                Ano,
                EstacaoId,
                AgendamentoId,
                StatusId,
                MotivoCancelamentoId,
                RegiaoId,
                TipoOS,
                Prioridade,
                Email,
                Nome,
                Telefone,
                NumeroDocumento,
                DataAgendamento,
                DataCancelamento,
                DataDespacho,
                DataDespachoProgramado,
                DataFinalizacao,
                DataParalisacao,
                DataInicioExecucao,
                Observacao
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Codigo,
                @Numero,
                @SubOS,
                @Ano,
                @EstacaoId,
                @AgendamentoId,
                @StatusId,
                @MotivoCancelamentoId,
                @RegiaoId,
                @TipoOS,
                @Prioridade,
                @Email,
                @Nome,
                @Telefone,
                @NumeroDocumento,
                @DataAgendamento,
                @DataCancelamento,
                @DataDespacho,
                @DataDespachoProgramado,
                @DataFinalizacao,
                @DataParalisacao,
                @DataInicioExecucao,
                @Observacao
            );
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, ordemServico, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(OrdemServico os, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico
            SET
                Codigo = @Codigo,
                Numero = @Numero,
                SubOS = @SubOS,
                Ano = @Ano,
                EstacaoId = @EstacaoId,
                AgendamentoId = @AgendamentoId,
                StatusId = @StatusId,
                MotivoCancelamentoId = @MotivoCancelamentoId,
                RegiaoId = @RegiaoId,
                TipoOS = @TipoOS,
                Prioridade = @Prioridade,
                Email = @Email,
                Nome = @Nome,
                Telefone = @Telefone,
                NumeroDocumento = @NumeroDocumento,
                DataAgendamento = @DataAgendamento,
                DataCancelamento = @DataCancelamento,
                DataDespacho = @DataDespacho,
                DataDespachoProgramado = @DataDespachoProgramado,
                DataFinalizacao = @DataFinalizacao,
                DataParalisacao = @DataParalisacao,
                DataSolicitacao = @DataSolicitacao,
                DataInicioExecucao = @DataInicioExecucao,
                Observacao = @Observacao
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, os, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<OrdemServico?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT 
                os.*,
                r.Descricao AS Regiao,  
                a.Descricao AS Agendamento,
                s.Descricao AS Status,
                mc.Descricao AS MotivoCancelamento,
                es.Nome AS Estacao
            FROM OrdemServico os
            LEFT JOIN Regiao r ON r.Id = os.RegiaoId
            LEFT JOIN Agendamento a ON a.Id = os.AgendamentoId
            LEFT JOIN StatusOrdemServico s ON s.Id = os.StatusId
            LEFT JOIN MotivoCancelamento mc ON mc.Id = os.MotivoCancelamentoId
            LEFT JOIN Estacao es ON es.Id = os.EstacaoId
            WHERE os.Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<OrdemServico>(new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, Guid statusId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico
            SET StatusId = @StatusId
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, StatusId = statusId }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<OrdemServico>> PaginatedGetAsync(OrdemServicoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT 
                OS.*,
                R.Descricao AS Regiao,
                A.Descricao AS Agendamento,
                S.Descricao AS Status
            FROM OrdemServico OS
            LEFT JOIN Regiao R ON R.Id = OS.RegiaoId
            LEFT JOIN Agendamento A ON A.Id = OS.AgendamentoId
            LEFT JOIN StatusOrdemServico S ON S.Id = OS.StatusId
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(OS.CODIGO) LIKE '%' || @TODOS || '%'
                    OR CAST(OS.NUMERO AS VARCHAR(50)) LIKE '%' || @TODOS || '%'
                    OR CAST(OS.DATASOLICITACAO AS VARCHAR(50)) LIKE '%' || @TODOS || '%'
                )",
                "@TODOS", filter.Todos.Trim().ToUpper());
        }



        var orderBy = filter.OrdenarPor switch
        {
            EOrdemServico.Numero => "OS.NUMERO",
            EOrdemServico.DataSolicitacao => "OS.DATASOLICITACAO",
            EOrdemServico.Status => "OS.STATUSID",
            _ => "OS.NUMERO"
        };

        if (!string.IsNullOrWhiteSpace(filter.Numero))
            builder.Where("UPPER(OS.NUMERO) LIKE '%' || @NUMERO || '%'", "@NUMERO", filter.Numero.ToUpper());

        var query = $@"
            {builder.Build(baseQuery)}
            ORDER BY {orderBy} {(filter.Ordem == EAscDesc.Asc ? "ASC" : "DESC")}
            OFFSET (@Pagina - 1) * @ItensPagina ROWS
            FETCH NEXT @ItensPagina ROWS ONLY
        ";

        var queryCount = $"SELECT COUNT(*) FROM ({builder.Build(baseQuery)}) A";

        builder.Parameters.Add("@Pagina", filter.Pagina);
        builder.Parameters.Add("@ItensPagina", filter.ItensPagina);

        var lista = await connection.QueryAsync<OrdemServico>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<OrdemServico>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<int> GetNextNumberOSAynsc(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"SELECT ISNULL(MAX(Numero),0) + 1 FROM OrdemServico WHERE Ano = @Ano";

        return await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(sql, new { Ano = DateTime.Now.Year }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<OrdemServico>> GetByAddressAsync(string busca, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"SELECT * FROM (
					        SELECT 
						        os.*,
								so.Descricao AS Status,
						        rd.Descricao AS Regiao,
								a.Descricao as Agendamento,
								--ss.Descricao AS ServicoSolicitado,
								--e.Descricao AS Equipe,
								--se.Descricao AS ServicoExecutado,								
								mc.Descricao AS MotivoCancelamento
					            FROM OrdemServico os
								LEFT JOIN StatusOrdemServico so           ON so.Id  = os.StatusId
					            LEFT JOIN Regiao rd                       ON rd.Id  = os.RegiaoId
								LEFT JOIN Agendamento a                   ON a.Id   = os.AgendamentoId
								--LEFT JOIN ServicoSolicitado ss          ON ss.Id  = os.ServicoSolicitadoId
								--LEFT JOIN Equipe e				      ON e.Id   = os.EquipeId
								--LEFT JOIN ServicoExecutado se           ON se.Id  = os.ServicoExecutadoId
								LEFT JOIN MotivoCancelamento mc           ON mc.Id  = os.MotivoCancelamentoId
                                ) AS Resultado						
                                WHERE CONCAT(TRIM(ENDERECO), ' ', TRIM(NUMERO), ' ', TRIM(BAIRRO)) LIKE '%' + @BUSCA + '%'";

        busca = busca.Replace(',', ' ').Replace('-', ' ').Trim();

        return await connection.QueryAsync<OrdemServico>(new CommandDefinition(sql, new { Busca = busca }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<OrdemServico>> GetOrdemServicoNearByAsync(string lat, string lon, decimal raioKm, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT os.*,
					    so.Descricao AS Status,
					    r.Descricao AS Regiao,
					    a.Descricao as Agendamento,
					    --ss.Descricao AS ServicoSolicitado,
					    --e.Descricao AS Equipe,
					    --se.Descricao AS ServicoExecutado,								
					    mc.Descricao AS MotivoCancelamento
					    FROM OrdemServico os
					    LEFT JOIN StatusOrdemServico so           ON so.Id  = os.StatusId
					    LEFT JOIN Regiao r                        ON r.Id   = os.RegiaoId
					    LEFT JOIN Agendamento a                   ON a.Id   = os.AgendamentoId
					    --LEFT JOIN ServicoSolicitado ss          ON ss.Id  = os.ServicoSolicitadoId
					    --LEFT JOIN Equipe e				      ON e.Id   = os.EquipeId
					    LEFT JOIN MotivoCancelamento mc           ON mc.Id  = os.MotivoCancelamentoId
                WHERE
                (
                    6371 * ACOS(
                        COS(RADIANS(@Lat)) *
                        COS(RADIANS(CAST(os.Lat AS FLOAT))) *
                        COS(RADIANS(CAST(os.Long AS FLOAT)) - RADIANS(@Long)) +
                        SIN(RADIANS(@Lat)) *
                        SIN(RADIANS(CAST(os.Lat AS FLOAT)))
                    )
                ) <= @RaioKm;";

        return await connection.QueryAsync<OrdemServico>(new CommandDefinition(sql, new { Lat = lat, Long = lon, RaioKm = raioKm }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<List<Regiao>> GetAllRegioesOrdemServicoAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Regiao
            ORDER BY Nome;
        ";

        var result = await connection.QueryAsync<Regiao>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<Guid> AddOrdemServicoServicoSolicitadoAsync(OrdemServicoServicoSolicitado ordemServicoServicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var sql = "INSERT INTO OrdemServicoServicoSolicitado (OrdemServicoId, ServicoSolicitadoId) VALUES (@OrdemServicoId, @ServicoSolicitadoId);";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, ordemServicoServicoSolicitado, transaction, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddOrdemServicoEquipamentoAsync(OrdemServicoEquipamento ordemServicoEquipamento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var sql = "INSERT INTO OrdemServicoEquipamento (OrdemServicoId, EquipamentoId) VALUES (@OrdemServicoId, @EquipamentoId);";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, ordemServicoEquipamento, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<OrdemServico>> GetAllByEquipamentoIdAndDateAsync(Guid id, DateTime date, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT OS.*
            FROM OrdemServico OS
            JOIN OrdemServicoEquipamento OSE ON OSE.OrdemServicoId = OS.ID
            WHERE OSE.EquipamentoId = @EquipamentoId
            AND DataFinalizacao >= @Data
            AND DataCancelamento IS NULL;
        ";

        var result = await connection.QueryAsync<OrdemServico>(new CommandDefinition(sql, new { EquipamentoId = id, Data = date }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }
}
