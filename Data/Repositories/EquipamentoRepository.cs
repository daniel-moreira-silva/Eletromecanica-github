namespace Data.Repositories;

public class EquipamentoRepository(DbConnection connection) : IEquipamentoRepository
{
    public async Task<Guid> AddAsync(Equipamento equipamento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DECLARE @Inserted TABLE (Id UNIQUEIDENTIFIER);

            INSERT INTO Equipamento
            (
                EstacaoId,
                TipoEquipamentoId,
                EquipamentoPrincipalId,
                Nome,
                Tag,
                Fabricante,
                Modelo,
                NumeroSerie,
                Observacoes
            )
            OUTPUT INSERTED.Id INTO @Inserted(Id)
            VALUES
            (
                @EstacaoId,
                @TipoEquipamentoId,
                @EquipamentoPrincipalId,
                @Nome,
                @Tag,
                @Fabricante,
                @Modelo,
                @NumeroSerie,
                @Observacoes
            );

            SELECT Id FROM @Inserted;
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, equipamento, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Equipamento equipamento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Equipamento
            SET
                EstacaoId = @EstacaoId,
                TipoEquipamentoId = @TipoEquipamentoId,
                EquipamentoPrincipalId = @EquipamentoPrincipalId,
                Nome = @Nome,
                Tag = @Tag,
                Fabricante = @Fabricante,
                Modelo = @Modelo,
                NumeroSerie = @NumeroSerie,
                Observacoes = @Observacoes
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, equipamento, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<List<TipoEquipamento>> GetAllTiposEquipamentoAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Nome, Categoria, Nivel, Ativo
            FROM TipoEquipamento
            WHERE Ativo = 1
            ORDER BY Nivel, Categoria, Nome;
        ";

        var result = await connection.QueryAsync<TipoEquipamento>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Equipamento
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Ativo = ativo }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Equipamento?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                Id,
                EstacaoId,
                TipoEquipamentoId,
                EquipamentoPrincipalId,
                Nome,
                Tag,
                Fabricante,
                Modelo,
                NumeroSerie,
                Observacoes,
                DataCriacao,
                Ativo
            FROM Equipamento
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Equipamento>(new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<ListaPaginada<Equipamento>> PaginatedGetAsync(EquipamentoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT 
                E.*,
                ES.NOME AS ESTACAO,
                EP.NOME AS EQUIPAMENTOPRINCIPAL,
                TE.NOME AS TIPOEQUIPAMENTO,
                TE.CATEGORIA AS CATEGORIA,
                TE.NIVEL AS NIVEL
            FROM EQUIPAMENTO E
            JOIN ESTACAO ES ON E.ESTACAOID = ES.ID
            JOIN TIPOEQUIPAMENTO TE ON E.TIPOEQUIPAMENTOID = TE.ID
            LEFT JOIN EQUIPAMENTO EP ON E.EQUIPAMENTOPRINCIPALID = EP.ID
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(TE.NOME) LIKE '%' + @TODOS + '%'
                    OR UPPER(TE.CATEGORIA) LIKE '%' + @TODOS + '%'
                    OR UPPER(ES.NOME) LIKE '%' + @TODOS + '%'
                    OR UPPER(E.NOME) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(E.TAG, '')) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(E.FABRICANTE, '')) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(E.MODELO, '')) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(E.NUMEROSERIE, '')) LIKE '%' + @TODOS + '%'
                )", "@TODOS", filter.Todos.Trim().ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filter.TipoEquipamento))
            builder.Where("UPPER(TE.NOME) LIKE '%' + @TIPOEQUIPAMENTO + '%'",
                          "@TIPOEQUIPAMENTO", filter.TipoEquipamento.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Estacao))
            builder.Where("UPPER(ES.NOME) LIKE '%' + @ESTACAO + '%'",
                          "@ESTACAO", filter.Estacao.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Tag))
            builder.Where("UPPER(ISNULL(E.TAG,'')) LIKE '%' + @TAG + '%'",
                          "@TAG", filter.Tag.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Fabricante))
            builder.Where("UPPER(ISNULL(E.FABRICANTE,'')) LIKE '%' + @FABRICANTE + '%'",
                          "@FABRICANTE", filter.Fabricante.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Modelo))
            builder.Where("UPPER(ISNULL(E.MODELO,'')) LIKE '%' + @MODELO + '%'",
                          "@MODELO", filter.Modelo.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.NumeroSerie))
            builder.Where("UPPER(ISNULL(E.NUMEROSERIE,'')) LIKE '%' + @NUMEROSERIE + '%'",
                          "@NUMEROSERIE", filter.NumeroSerie.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Nome))
            builder.Where("UPPER(E.NOME) LIKE '%' + @NOME + '%'",
                          "@NOME", filter.Nome.Trim().ToUpper());

        // Novo: filtrar por Principal/Componente
        // Sugestões de campos no filtro:
        //  - filter.SomentePrincipais (bool?)
        //  - filter.SomenteComponentes (bool?)
        if (filter.SomentePrincipais == true)
            builder.Where("E.EQUIPAMENTOPRINCIPALID IS NULL");

        if (filter.SomenteComponentes == true)
            builder.Where("E.EQUIPAMENTOPRINCIPALID IS NOT NULL");

        if (filter.NivelTipoEquipamento.HasValue)
            builder.Where("TE.NIVEL = @NIVELTIPO", "@NIVELTIPO", filter.NivelTipoEquipamento.Value);

        if (filter.Ativo.HasValue)
            builder.Where("E.ATIVO = @ATIVO", "@ATIVO", filter.Ativo);

        var orderBy = filter.OrdenarPor switch
        {
            EEquipamento.Nome => "E.NOME",
            EEquipamento.Estacao => "ES.NOME",
            EEquipamento.TipoEquipamento => "TE.NOME",
            EEquipamento.Tag => "E.TAG",
            EEquipamento.Fabricante => "E.FABRICANTE",
            EEquipamento.Modelo => "E.MODELO",
            EEquipamento.NumeroSerie => "E.NUMEROSERIE",
            _ => "E.NOME"
        };

        var query = $@"
            {builder.Build(baseQuery)}
            ORDER BY {orderBy} {(filter.Ordem == EAscDesc.Asc ? "ASC" : "DESC")}
            OFFSET (@PAGINA - 1) * @ITENSPAGINA ROWS
            FETCH NEXT @ITENSPAGINA ROWS ONLY
        ";

        var queryCount = $@"
            SELECT COUNT(*)
            FROM ({builder.Build(baseQuery)}) AS A
        ";

        builder.Parameters.Add("@PAGINA", filter.Pagina);
        builder.Parameters.Add("@ITENSPAGINA", filter.ItensPagina);

        var result = await connection.QueryAsync<Equipamento>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var resultCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var paginas = resultCount % filter.ItensPagina > 0
            ? (resultCount / filter.ItensPagina) + 1
            : resultCount / filter.ItensPagina;

        if (paginas == 0) paginas = 1;

        return new ListaPaginada<Equipamento>
        {
            Lista = [.. result],
            Paginas = paginas,
            TotalItens = resultCount
        };
    }

    public async Task<List<Equipamento>> GetAllPrincipalEquipmentsByEstacaoIdAsync(Guid estacaoId, bool? principal = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"
            SELECT EP.NOME AS EQUIPAMENTOPRINCIPAL, E.Id, E.Nome, E.Tag
            FROM Equipamento E
            LEFT JOIN EQUIPAMENTO EP ON E.EQUIPAMENTOPRINCIPALID = EP.ID
            WHERE E.Ativo = 1";

        if (principal.HasValue)
            if (principal.Value)
                sql += " AND E.EquipamentoPrincipalId IS NULL ";
            else
                sql += " AND E.EquipamentoPrincipalId IS NOT NULL ";

        sql += "ORDER BY E.Nome, E.Tag;";

        var result = await connection.QueryAsync<Equipamento>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<Equipamento>> GetAllComponentsByPrincipalEquipmentIdAsync(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Nome, Tag
            FROM Equipamento
            WHERE Ativo = 1 AND EquipamentoPrincipalId = @equipamentoId
            ORDER BY Nome, Tag;
        ";

        var result = await connection.QueryAsync<Equipamento>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<Equipamento>?> GetAllEquipamentosByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Equipamento E
            JOIN OrdemServicoEquipamento OSE ON OSE.EquipamentoId = E.Id
            WHERE OSE.OrdemServicoId = @Id;
        ";

        var result = await connection.QueryAsync<Equipamento>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }
}