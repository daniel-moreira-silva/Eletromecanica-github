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
                CustoTotal,
                Observacao,
                OrdemServicoPaiId
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
                @CustoTotal,
                @Observacao,
                @OrdemServicoPaiId
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
                CustoTotal = @CustoTotal,
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

    public async Task<ListaPaginada<OrdemServicoList>> PaginatedGetAsync(OrdemServicoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                OS.*,
                R.Descricao AS Regiao,
                ES.Nome AS Estacao,
                ES.Endereco AS Endereco,
                A.Descricao AS Agendamento,
                S.Descricao AS Status,
                MC.Descricao AS MotivoCancelamento,
                F.Nome AS Funcionario
            FROM OrdemServico OS
            LEFT JOIN Regiao R ON R.Id = OS.RegiaoId
            LEFT JOIN Agendamento A ON A.Id = OS.AgendamentoId
            LEFT JOIN StatusOrdemServico S ON S.Id = OS.StatusId
            LEFT JOIN MotivoCancelamento MC ON MC.Id = OS.MotivoCancelamentoId
            LEFT JOIN Estacao ES ON ES.Id = OS.EstacaoId
            LEFT JOIN Funcionario F ON F.Id = os.FuncionarioId
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
            builder.Where(@"
                (
                    UPPER(OS.Codigo) LIKE '%' + @Todos + '%'
                    OR CAST(OS.Numero AS VARCHAR(50)) LIKE '%' + @Todos + '%'
                    OR CAST(OS.DataSolicitacao AS VARCHAR(50)) LIKE '%' + @Todos + '%'
                )",
                "@Todos", filter.Todos.Trim().ToUpper());

        var orderBy = filter.OrdenarPor switch
        {
            EOrdemServico.Numero => "OS.Numero",
            EOrdemServico.Codigo => "OS.DataSolicitacao",
            EOrdemServico.Estacao => "ES.Nome",
            EOrdemServico.Funcionario => "F.Nome",
            EOrdemServico.Endereco => "ES.Endereco",
            EOrdemServico.Agendamento => "A.Descricao",
            EOrdemServico.MotivoCancelamento => "MC.Descricao",
            EOrdemServico.TipoOS => "OS.TipoOS",
            EOrdemServico.Prioridade => "OS.Prioridade",
            EOrdemServico.DataSolicitacao => "OS.DataSolicitacao",
            EOrdemServico.DataAgendamento => "OS.DataAgendamento",
            EOrdemServico.DataCancelamento => "OS.DataCancelamento",
            EOrdemServico.DataDespacho => "OS.DataDespacho",
            EOrdemServico.DataDespachoProgramado => "OS.DataDespachoProgramado",
            EOrdemServico.DataFinalizacao => "OS.DataFinalizacao",
            EOrdemServico.DataParalisacao => "OS.DataParalisacao",
            EOrdemServico.DataInicioExecucao => "OS.DataInicioExecucao",
            EOrdemServico.Status => "S.Descricao",
            _ => "OS.Numero"
        };

        AplicarFiltros(builder, filter);

        var query = $@"
            {builder.Build(baseQuery)}
            ORDER BY {orderBy} {(filter.Ordem == EAscDesc.Asc ? "ASC" : "DESC")}
            OFFSET (@Pagina - 1) * @ItensPagina ROWS
            FETCH NEXT @ItensPagina ROWS ONLY
        ";

        var queryCount = $"SELECT COUNT(*) FROM ({builder.Build(baseQuery)}) A";

        builder.Parameters.Add("@Pagina", filter.Pagina);
        builder.Parameters.Add("@ItensPagina", filter.ItensPagina);

        var lista = await connection.QueryAsync<OrdemServicoList>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<OrdemServicoList>
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

    public async Task<int> GetNextSubOSAsync(int numero, int ano, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"SELECT ISNULL(MAX(SubOs), 0) + 1 FROM OrdemServico WHERE Numero = @Numero AND Ano = @Ano";

        return await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(sql, new { Numero = numero, Ano = ano }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<OrdemServicoList>> GetSubOsListAsync(int numero, int ano, Guid excludeId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                OS.*,
                ES.Nome AS Estacao,
                ES.Endereco AS Endereco,
                S.Descricao AS Status,
                F.Nome AS Funcionario
            FROM OrdemServico OS
            LEFT JOIN Estacao ES ON ES.Id = OS.EstacaoId
            LEFT JOIN StatusOrdemServico S ON S.Id = OS.StatusId
            LEFT JOIN Funcionario F ON F.Id = OS.FuncionarioId
            WHERE OS.Numero = @Numero AND OS.Ano = @Ano AND OS.Id <> @ExcludeId
            ORDER BY OS.SubOs ASC
        ";

        return await connection.QueryAsync<OrdemServicoList>(
            new CommandDefinition(sql, new { Numero = numero, Ano = ano, ExcludeId = excludeId }, transaction, cancellationToken: cancellationToken)
        );
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

    public async Task DeleteServicosSolicitadosByOrdemServicoAsync(Guid ordemServicoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        const string sql = "DELETE FROM OrdemServicoServicoSolicitado WHERE OrdemServicoId = @OrdemServicoId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { OrdemServicoId = ordemServicoId }, transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteEquipamentosByOrdemServicoAsync(Guid ordemServicoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        const string sql = "DELETE FROM OrdemServicoEquipamento WHERE OrdemServicoId = @OrdemServicoId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { OrdemServicoId = ordemServicoId }, transaction, cancellationToken: cancellationToken));
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

    public async Task<IEnumerable<ListaCountOrdemServico>> ListaCountOrdemServicoAsync(OrdemServicoFilter filtro, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                COUNT(OS.Id) AS TotalOcorrencias,
                S.Id AS StatusId
            FROM StatusOrdemServico S
            LEFT JOIN OrdemServico OS ON OS.StatusId = S.Id
            LEFT JOIN Regiao R ON R.Id = OS.RegiaoId
            LEFT JOIN Agendamento A ON A.Id = OS.AgendamentoId
            LEFT JOIN MotivoCancelamento MC ON MC.Id = OS.MotivoCancelamentoId
            LEFT JOIN Estacao ES ON ES.Id = OS.EstacaoId
        ";

        var builder = new SqlQueryBuilder();

        AplicarFiltros(builder, filtro);

        var query = $@"
            {builder.Build(baseQuery)}
            GROUP BY S.Id
        ";

        var lista = await connection.QueryAsync<ListaCountOrdemServico>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );
        return lista;
    }

    public async Task<bool> CancelarOrdemServicoAsync(Guid ordemServicoId, Guid motivoCancelamentoId, string observacao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico 
            SET DataCancelamento = @DataCancelamento, 
            MotivoCancelamentoId = @MotivoCancelamentoId, 
            Observacao = CASE WHEN OBSERVACAO LIKE '' THEN @Observacao ELSE Observacao + '<BR><BR>' + @Observacao END, 
            StatusId = @StatusId
            WHERE Id = @Id
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = ordemServicoId, DataCancelamento = DateTime.Now, MotivoCancelamentoId = motivoCancelamentoId, Observacao = observacao, StatusId = Constantes.OrdemServicoStatusCancelada }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    private static void AplicarFiltros(SqlQueryBuilder builder, OrdemServicoFilter filtro)
    {
        if (!string.IsNullOrWhiteSpace(filtro.Numero))
            builder.Where("UPPER(CAST(OS.Codigo AS VARCHAR(50))) LIKE '%' + @Numero + '%'", "@Numero", filtro.Numero.ToUpper());

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            builder.Where("OS.Nome LIKE '%' + @Nome + '%'", "@Nome", filtro.Nome);

        if (!string.IsNullOrWhiteSpace(filtro.NumeroDocumento))
            builder.Where("OS.NumeroDocumento LIKE '%' + @NumeroDocumento + '%'", "@NumeroDocumento", filtro.NumeroDocumento);

        if (!string.IsNullOrWhiteSpace(filtro.Endereco))
            builder.Where("ES.Endereco LIKE '%' + @Endereco + '%'", "@Endereco", filtro.Endereco);

        if (!string.IsNullOrWhiteSpace(filtro.Bairro))
            builder.Where("ES.Bairro LIKE '%' + @Bairro + '%'", "@Bairro", filtro.Bairro);

        if (!string.IsNullOrWhiteSpace(filtro.PontoReferencia))
            builder.Where("ES.PontoReferencia LIKE '%' + @PontoReferencia + '%'", "@PontoReferencia", filtro.PontoReferencia);

        if (filtro.StatusId != null && filtro.StatusId.Any())
            builder.Where("OS.StatusId IN @StatusId", "@StatusId", filtro.StatusId);

        if (filtro.RegiaoId != null && filtro.RegiaoId.Any())
            builder.Where("OS.RegiaoId IN @RegiaoId", "@RegiaoId", filtro.RegiaoId);

        if (filtro.AgendamentoId != null && filtro.AgendamentoId.Any())
            builder.Where("OS.AgendamentoId IN @AgendamentoId", "@AgendamentoId", filtro.AgendamentoId);

        if (filtro.ServicoSolicitadoId != null && filtro.ServicoSolicitadoId.Any())
            builder.Where("EXISTS (SELECT 1 FROM OrdemServicoServicoSolicitado OSS WHERE OSS.OrdemServicoId = OS.Id AND OSS.ServicoSolicitadoId IN @ServicoSolicitadoId)", "@ServicoSolicitadoId", filtro.ServicoSolicitadoId);

        if (filtro.ServicoExecutadoId != null && filtro.ServicoExecutadoId.Any())
            builder.Where("EXISTS (SELECT 1 FROM OrdemServicoServicoExecutado OSE WHERE OSE.OrdemServicoId = OS.Id AND OSE.ServicoExecutadoId IN @ServicoExecutadoId)", "@ServicoExecutadoId", filtro.ServicoExecutadoId);

        if (filtro.MotivoCancelamentoId != null && filtro.MotivoCancelamentoId.Any())
            builder.Where("OS.MotivoCancelamentoId IN @MotivoCancelamentoId", "@MotivoCancelamentoId", filtro.MotivoCancelamentoId);

        AplicarFiltroPeriodo(builder, "OS.DataSolicitacao", filtro.DataSolicitacaoInicio, filtro.DataSolicitacaoFim, "DataSolicitacaoInicio", "DataSolicitacaoFim");
        AplicarFiltroPeriodo(builder, "OS.DataAgendamento", filtro.DataAgendamentoInicio, filtro.DataAgendamentoFim, "DataAgendamentoInicio", "DataAgendamentoFim");
        AplicarFiltroPeriodo(builder, "OS.DataDespacho", filtro.DataDespachoInicio, filtro.DataDespachoFim, "DataDespachoInicio", "DataDespachoFim");
        AplicarFiltroPeriodo(builder, "OS.DataFinalizacao", filtro.DataFinalizacaoInicio, filtro.DataFinalizacaoFim, "DataFinalizacaoInicio", "DataFinalizacaoFim");
        AplicarFiltroPeriodo(builder, "OS.DataParalisacao", filtro.DataParalisacaoInicio, filtro.DataParalisacaoFim, "DataParalisacaoInicio", "DataParalisacaoFim");
        AplicarFiltroPeriodo(builder, "OS.DataDespachoProgramado", filtro.DataDespachoProgramadoInicio, filtro.DataDespachoProgramadoFim, "DataDespachoProgramadoInicio", "DataDespachoProgramadoFim");
        AplicarFiltroPeriodo(builder, "OS.DataCancelamento", filtro.DataCancelamentoInicio, filtro.DataCancelamentoFim, "DataCancelamentoInicio", "DataCancelamentoFim");
    }

    private static void AplicarFiltroPeriodo(SqlQueryBuilder builder, string coluna, DateTime? inicio, DateTime? fim, string paramInicio, string paramFim)
    {
        if (inicio.HasValue && fim.HasValue)
            builder.Where($"CAST({coluna} AS DATE) BETWEEN @{paramInicio} AND @{paramFim}", new() { [$"@{paramInicio}"] = inicio.Value.Date, [$"@{paramFim}"] = fim.Value.Date });
        else if (inicio.HasValue)
            builder.Where($"CAST({coluna} AS DATE) >= @{paramInicio}", $"@{paramInicio}", inicio.Value.Date);
        else if (fim.HasValue)
            builder.Where($"CAST({coluna} AS DATE) <= @{paramFim}", $"@{paramFim}", fim.Value.Date);
    }

    public async Task<bool> IniciarOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico 
            SET DataDespacho = @DataDespacho,
            FuncionarioId = @FuncionarioId,
            StatusId = @StatusId
            WHERE Id = @Id
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = ordemServicoId, DataDespacho = DateTime.Now, FuncionarioId = funcionarioId, StatusId = Constantes.OrdemServicoStatusIniciada }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DespacharOrdemServicoAsync(Guid ordemServicoId, Guid funcionarioId, DateTime dataDespachoProgramado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico 
            SET DataDespachoProgramado = @DataDespachoProgramado,
            FuncionarioId = @FuncionarioId,
            IsProgramada = 1,
            StatusId = @StatusId
            WHERE Id = @Id
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = ordemServicoId, DataDespacho = DateTime.Now, FuncionarioId = funcionarioId, DataDespachoProgramado = dataDespachoProgramado, StatusId = Constantes.OrdemServicoStatusIniciada }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DevolverOrdemServicoAsync(Guid ordemServicoId, string observacaoDevolucao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico
            SET DataDespacho = NULL,
            DataDespachoProgramado = NULL,
            FuncionarioId = NULL,
            IsProgramada = 0,
            ObservacaoDevolucao = @ObservacaoDevolucao,
            StatusId = @StatusId
            WHERE Id = @Id
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = ordemServicoId, ObservacaoDevolucao = observacaoDevolucao, StatusId = Constantes.OrdemServicoStatusSolicitada }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> AtualizarPrioridadeAsync(Guid ordemServicoId, EPrioridade prioridade, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE OrdemServico
            SET Prioridade = @Prioridade
            WHERE Id = @Id
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = ordemServicoId, Prioridade = prioridade }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }
}
