namespace Data.Repositories;

internal class ServicoSolicitadoRepository(DbConnection connection) : IServicoSolicitadoRepository
{
    public async Task<Guid> AddAsync(ServicoSolicitado servicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO ServicoSolicitado (Codigo, Descricao, Prioridade, Sla)
            OUTPUT INSERTED.Id
            VALUES (@Codigo, @Descricao, @Prioridade, @Sla);
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, servicoSolicitado, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(ServicoSolicitado servicoSolicitado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE ServicoSolicitado
            SET
                Codigo = @Codigo,
                Descricao = @Descricao,
                Prioridade = @Prioridade,
                Sla = @Sla,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, servicoSolicitado, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ServicoSolicitado?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM ServicoSolicitado
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<ServicoSolicitado>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE ServicoSolicitado
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id, Ativo = active }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<ServicoSolicitadoList>> PaginatedGetAsync(ServicoSolicitadoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                Id,
                Codigo,
                Descricao,
                Prioridade,
                Sla,
                Ativo
            FROM ServicoSolicitado
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(Codigo) LIKE '%' + @TODOS + '%'
                    OR UPPER(Descricao) LIKE '%' + @TODOS + '%'
                )",
                "@TODOS", filter.Todos.Trim().ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filter.Codigo))
            builder.Where("UPPER(Codigo) LIKE '%' + @CODIGO + '%'",
                          "@CODIGO", filter.Codigo.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Descricao))
            builder.Where("UPPER(Descricao) LIKE '%' + @DESCRICAO + '%'",
                          "@DESCRICAO", filter.Descricao.Trim().ToUpper());

        if (filter.Ativo.HasValue)
            builder.Where("Ativo = @ATIVO", "@ATIVO", filter.Ativo.Value);

        var orderBy = filter.OrdenarPor switch
        {
            EServicoSolicitado.Codigo => "Codigo",
            EServicoSolicitado.Descricao => "Descricao",
            _ => "Codigo"
        };

        var query = $@"
            {builder.Build(baseQuery)}
            ORDER BY {orderBy} {(filter.Ordem == EAscDesc.Asc ? "ASC" : "DESC")}
            OFFSET (@Pagina - 1) * @ItensPagina ROWS
            FETCH NEXT @ItensPagina ROWS ONLY
        ";

        var queryCount = $"SELECT COUNT(*) FROM ({builder.Build(baseQuery)}) A";

        builder.Parameters.Add("@Pagina", filter.Pagina);
        builder.Parameters.Add("@ItensPagina", filter.ItensPagina);

        var lista = await connection.QueryAsync<ServicoSolicitadoList>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<ServicoSolicitadoList>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM ServicoSolicitado WHERE Codigo = @Codigo";

        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { codigo, id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }

    public async Task<List<ServicoSolicitado>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM ServicoSolicitado
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<ServicoSolicitado>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<ServicoSolicitado>> GetAllByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM ServicoSolicitado
            JOIN OrdemServicoServicoSolicitado SS ON SS.ServicoSolicitadoId = ServicoSolicitado.Id
            WHERE SS.OrdemServicoId = @Id;
        ";

        var result = await connection.QueryAsync<ServicoSolicitado>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<IEnumerable<OrdemServicoServicoSolicitado>> GetAllByOrdensServicoIdsAsync(List<Guid> ordensServicoIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        if (ordensServicoIds is null || ordensServicoIds.Count == 0)
            return [];

        const string sql = @"
            SELECT *
            FROM OrdemServicoServicoSolicitado
            WHERE OrdemServicoId IN @Ids
        ";

        return await connection.QueryAsync<OrdemServicoServicoSolicitado>(new CommandDefinition(sql, new { Ids = ordensServicoIds }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<List<ServicoSolicitado>> GetAllByRegraIdAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT SS.*
            FROM ServicoSolicitado SS
            JOIN RegraPreventivaServicoSolicitado RP ON RP.ServicoSolicitadoId = SS.Id
            WHERE RP.RegraPreventivaId = @RegraId;
        ";

        var result = await connection.QueryAsync<ServicoSolicitado>(new CommandDefinition(sql, new { RegraId = id }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<ServicoSolicitado>> GetAllByIdListAsync(List<Guid> ids, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM ServicoSolicitado
            WHERE Id IN @Ids;
        ";

        var result = await connection.QueryAsync<ServicoSolicitado>(new CommandDefinition(sql, new { ids }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }
}
