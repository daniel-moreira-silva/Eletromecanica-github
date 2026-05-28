namespace Data.Repositories;

internal class MaterialRepository(DbConnection connection) : IMaterialRepository
{
    public async Task<Guid> AddAsync(Material material, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Material (Codigo, Descricao, UnidadeMedida)
            OUTPUT INSERTED.Id
            VALUES (@Codigo, @Descricao, @UnidadeMedida);
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, material, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Material material, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Material
            SET
                Codigo = @Codigo,
                Descricao = @Descricao,
                UnidadeMedida = @UnidadeMedida,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, material, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Material?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Material
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Material>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Material
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id, Ativo = active }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<Material>> PaginatedGetAsync(MaterialFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                Id,
                Codigo,
                Descricao,
                UnidadeMedida,
                Ativo
            FROM Material
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(Codigo) LIKE '%' + @TODOS + '%'
                    OR UPPER(Descricao) LIKE '%' + @TODOS + '%'
                    OR UPPER(UnidadeMedida) LIKE '%' + @TODOS + '%'
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
            EMaterial.Codigo => "Codigo",
            EMaterial.Descricao => "Descricao",
            _ => "Descricao"
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

        var lista = await connection.QueryAsync<Material>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<Material>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<List<Material>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Material
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<Material>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM Material WHERE Descricao = @Descricao";

        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { descricao, id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }
}
