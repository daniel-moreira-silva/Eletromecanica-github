namespace Data.Repositories;

public class DashboardRepository(DbConnection connection) : IDashboardRepository
{
    // Status OS
    public async Task<DashboardStatusOsDto> ObterStatusOsAsync(
        Guid? estacaoId,
        CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);

        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        var sql = $@"
            SELECT
                SUM(CASE WHEN os.StatusId NOT IN (@StatusFinalizada, @StatusCancelada)
                          AND os.DataPrevista IS NULL  THEN 1 ELSE 0 END) AS Abertas,
                SUM(CASE WHEN os.StatusId = @StatusFinalizada              THEN 1 ELSE 0 END) AS Concluidas,
                SUM(CASE WHEN os.StatusId NOT IN (@StatusFinalizada, @StatusCancelada)
                          AND os.DataPrevista IS NOT NULL
                          AND os.DataPrevista < @Hoje  THEN 1 ELSE 0 END) AS Atrasadas,
                -- EmAndamento = qualquer status que não seja Finalizada ou Cancelada
                SUM(CASE WHEN os.StatusId NOT IN (@StatusFinalizada, @StatusCancelada)
                                                       THEN 1 ELSE 0 END) AS EmAndamento
            FROM OrdemServico os
            WHERE os.Ano = @AnoAtual
              {filtro}
        ";

        return await connection.QueryFirstOrDefaultAsync<DashboardStatusOsDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        ) ?? new DashboardStatusOsDto();
    }

    // MTTR e MTBF
    public async Task<DashboardMttrDto> ObterMttrAsync(Guid? estacaoId, CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);
        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        var sql = $@"
        SELECT
            FORMAT(os.DataSolicitacao, 'MMM', 'pt-BR') AS Mes,
            ROUND(
                ISNULL(AVG(CAST(DATEDIFF(MINUTE, os.DataSolicitacao, os.DataFinalizacao) AS FLOAT)) / 60.0, 0)
            , 1) AS Valor,
            MONTH(os.DataSolicitacao) AS NumeroMes
        FROM OrdemServico os
        WHERE os.StatusId       = @StatusFinalizada
          AND os.DataFinalizacao IS NOT NULL
          AND os.DataSolicitacao >= @DataCorte
          {filtro}
        GROUP BY FORMAT(os.DataSolicitacao, 'MMM', 'pt-BR'), MONTH(os.DataSolicitacao)
        ORDER BY NumeroMes ASC";

        var serie = (await connection.QueryAsync<DashboardSerieMensalDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        )).ToList();

        return new DashboardMttrDto
        {
            Atual = serie.LastOrDefault()?.Valor ?? 0,
            Serie = serie
        };
    }

    public async Task<DashboardMtbfDto> ObterMtbfAsync(Guid? estacaoId, CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);
        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        var sql = $@"
        WITH IntervalosEntrefalhas AS (
            SELECT
                os.DataSolicitacao,
                DATEDIFF(HOUR, os.DataFinalizacao,
                    LEAD(os.DataSolicitacao) OVER (
                        PARTITION BY ose.EquipamentoId
                        ORDER BY os.DataSolicitacao
                    )
                ) AS IntervaloHoras
            FROM OrdemServico os
            JOIN OrdemServicoEquipamento ose ON ose.OrdemServicoId = os.Id
            WHERE os.StatusId       = @StatusFinalizada
              AND os.TipoOS         = 0
              AND os.DataFinalizacao IS NOT NULL
              AND os.DataSolicitacao >= @DataCorte
              {filtro}
        )
        SELECT
            FORMAT(DataSolicitacao, 'MMM', 'pt-BR') AS Mes,
            ROUND(ISNULL(AVG(CAST(IntervaloHoras AS FLOAT)), 0), 1) AS Valor,
            MONTH(DataSolicitacao) AS NumeroMes
        FROM IntervalosEntrefalhas
        WHERE IntervaloHoras IS NOT NULL
        GROUP BY FORMAT(DataSolicitacao, 'MMM', 'pt-BR'), MONTH(DataSolicitacao)
        ORDER BY NumeroMes ASC";

        var serie = (await connection.QueryAsync<DashboardSerieMensalDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        )).ToList();

        return new DashboardMtbfDto
        {
            Atual = serie.LastOrDefault()?.Valor ?? 0,
            Serie = serie
        };
    }

    // Disponibilidade por ativo
    public async Task<DashboardDisponibilidadeDto> ObterDisponibilidadeAsync(
        Guid? estacaoId,
        CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);

        var (_, parametros) = DbUtils.MontarBase(estacaoId);

        // Equipamentos principais = EquipamentoPrincipalId IS NULL
        // TipoOS = 0 (Corretiva) — tempo fora representa indisponibilidade
        var sql = $@"
            SELECT
                e.Nome  AS NomeAtivo,
                e.Tag,
                CAST(
                    CASE
                        WHEN SUM(
                                DATEDIFF(MINUTE, os.DataSolicitacao, ISNULL(os.DataFinalizacao, GETDATE()))
                             ) IS NULL
                        THEN 100.0
                        ELSE 100.0 - (
                            SUM(CAST(DATEDIFF(MINUTE, os.DataSolicitacao, ISNULL(os.DataFinalizacao, GETDATE())) AS FLOAT))
                            / (720.0 * 60.0)
                        ) * 100.0
                    END
                AS DECIMAL(5,2)) AS Disponibilidade
            FROM Equipamento e
            LEFT JOIN OrdemServicoEquipamento ose ON ose.EquipamentoId = e.Id
            LEFT JOIN OrdemServico os
              ON  os.Id                      = ose.OrdemServicoId
              AND os.TipoOS                  = 0
              AND os.Ano                     = @AnoAtual
              AND MONTH(os.DataSolicitacao)  = @MesAtual
              AND os.StatusId               != @StatusCancelada
            WHERE e.EquipamentoPrincipalId IS NULL
              AND e.Ativo                  = 1
              {(estacaoId.HasValue ? "AND e.EstacaoId = @EstacaoId" : string.Empty)}
            GROUP BY e.Id, e.Nome, e.Tag
            ORDER BY Disponibilidade ASC
        ";

        var ativos = (await connection.QueryAsync<DashboardDisponibilidadeAtivoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        )).ToList();

        return new DashboardDisponibilidadeDto
        {
            Geral = ativos.Any()
                ? Math.Round(ativos.Average(a => a.Disponibilidade), 1)
                : 0,
            Ativos = ativos
        };
    }

    // Motivação mensal
    public async Task<DashboardMotivacaoDto> ObterMotivacaoAsync(
        Guid? estacaoId,
        CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);

        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        // TipoOS: 0=Corretiva, 1=Preventiva, 2=Preditiva, 3=Inspecao
        var sqlSerie = $@"
            SELECT
                FORMAT(os.DataSolicitacao, 'MMM', 'pt-BR')                             AS Mes,
                SUM(CASE WHEN os.TipoOS = 0 THEN 1 ELSE 0 END)                         AS Corretivas,
                SUM(CASE WHEN os.TipoOS = 1 THEN 1 ELSE 0 END)                         AS Preventivas,
                SUM(CASE WHEN os.TipoOS = 2 THEN 1 ELSE 0 END)                         AS Preditivas,
                MONTH(os.DataSolicitacao)                                               AS NumeroMes
            FROM OrdemServico os
            WHERE os.DataSolicitacao >= @DataCorte
              AND os.StatusId        != @StatusCancelada
              {filtro}
            GROUP BY
                FORMAT(os.DataSolicitacao, 'MMM', 'pt-BR'),
                MONTH(os.DataSolicitacao)
            ORDER BY NumeroMes ASC
        ";

        var sqlTotais = $@"
            SELECT
                SUM(CASE WHEN os.TipoOS = 0 THEN 1 ELSE 0 END) AS CorretivasMes,
                SUM(CASE WHEN os.TipoOS = 1 THEN 1 ELSE 0 END) AS PreventivasMes,
                SUM(CASE WHEN os.TipoOS = 2 THEN 1 ELSE 0 END) AS PreditivasMes
            FROM OrdemServico os
            WHERE os.Ano                     = @AnoAtual
              AND MONTH(os.DataSolicitacao)  = @MesAtual
              AND os.StatusId               != @StatusCancelada
              {filtro}
        ";

        var serie = (await connection.QueryAsync<DashboardSerieMotivacaoDto>(
            new CommandDefinition(sqlSerie, parametros, cancellationToken: ct)
        )).ToList();

        var totais = await connection.QueryFirstOrDefaultAsync<DashboardMotivacaoDto>(
            new CommandDefinition(sqlTotais, parametros, cancellationToken: ct)
        ) ?? new DashboardMotivacaoDto();

        totais.Serie = serie;

        return totais;
    }

    // Custos
    public async Task<DashboardCustosDto> ObterCustosAsync(
        Guid? estacaoId,
        CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);

        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        // CustoTotal foi adicionado à OrdemServico — NULL enquanto não preenchido
        var sql = $@"
            SELECT
                ISNULL(SUM(os.CustoTotal), 0)  AS TotalMes,
                ISNULL(AVG(os.CustoTotal), 0)  AS MedioPorOs
            FROM OrdemServico os
            WHERE os.Ano                     = @AnoAtual
              AND MONTH(os.DataSolicitacao)  = @MesAtual
              AND os.StatusId               != @StatusCancelada
              {filtro}
        ";

        return await connection.QueryFirstOrDefaultAsync<DashboardCustosDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        ) ?? new DashboardCustosDto();
    }

    // Estoque
    public Task<DashboardEstoqueDto> ObterEstoqueAsync(CancellationToken ct = default)
    {
        // TODO: implementar quando o módulo de almoxarifado (PecaEstoque) for criado.
        // A tabela PecaEstoque ainda não existe no schema atual.
        return Task.FromResult(new DashboardEstoqueDto());
    }

    // OS Atrasadas
    public async Task<List<DashboardOsAtrasadaDto>> ObterOsAtrasadasAsync(
        Guid? estacaoId,
        CancellationToken ct = default)
    {
        await DbUtils.EnsureOpenAsync(connection, ct);

        var (filtro, parametros) = DbUtils.MontarBase(estacaoId);

        // DataPrevista foi adicionado à OrdemServico
        // Vínculo com equipamento é via OrdemServicoEquipamento (N:N)
        // Pega apenas o primeiro equipamento vinculado (TOP 1 no sub-select) para exibição
        var sql = $@"
            SELECT TOP 10
                CAST(os.Numero AS NVARCHAR(20))                   AS Numero,
                ISNULL(
                    (
                        SELECT TOP 1 e2.Nome
                        FROM OrdemServicoEquipamento ose2
                        JOIN Equipamento e2 ON e2.Id = ose2.EquipamentoId
                        WHERE ose2.OrdemServicoId = os.Id
                        ORDER BY e2.Nome
                    ), N'Não informado'
                )                                                 AS NomeAtivo,
                DATEDIFF(DAY, os.DataPrevista, @Hoje)             AS DiasAtraso,
                CASE os.TipoOS
                    WHEN 0 THEN N'Corretiva'
                    WHEN 1 THEN N'Preventiva'
                    WHEN 2 THEN N'Preditiva'
                    ELSE        N'Inspeção'
                END                                               AS Motivacao,
                so.Descricao                                      AS Status
            FROM OrdemServico os
            JOIN StatusOrdemServico so ON so.Id = os.StatusId
            WHERE os.DataPrevista   IS NOT NULL
              AND os.DataPrevista    < @Hoje
              AND os.StatusId       != @StatusFinalizada
              AND os.StatusId       != @StatusCancelada
              {filtro}
            ORDER BY DiasAtraso DESC
        ";

        var result = await connection.QueryAsync<DashboardOsAtrasadaDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)
        );

        return result.ToList();
    }
}