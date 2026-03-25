namespace Data.Repositories;

public class ServicoExecutadoRepository(DbConnection connection) : IServicoExecutadoRepository
{
    public async Task<Guid> AddAsync(ServicoExecutado servicoExecutado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO ServicoExecutado (Codigo, Descricao)
            OUTPUT INSERTED.Id
            VALUES (@Codigo, @Descricao);
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, servicoExecutado, transaction, cancellationToken: cancellationToken));
    }

    public async Task<List<ServicoExecutado>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Codigo, Descricao, Ativo
            FROM ServicoExecutado
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<ServicoExecutado>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<ServicoExecutado>> GetAllByOrdemServicoAsync(Guid? id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                Id,
                Codigo,
                Descricao,
                Ativo
            FROM ServicoExecutado
            JOIN OrdemServicoServicoExecutado SE ON SE.ServicoExecutadoId = ServicoExecutado.Id
            WHERE SE.OrdemServicoId = @Id;
        ";

        var result = await connection.QueryAsync<ServicoExecutado>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<ServicoExecutado?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                Id,
                Codigo,
                Descricao,
                Ativo
            FROM ServicoExecutado
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<ServicoExecutado>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<ListaPaginada<ServicoExecutado>> PaginatedGetAsync(ServicoExecutadoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                Id,
                Codigo,
                Descricao,
                Ativo
            FROM ServicoExecutado
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
            EServicoExecutado.Codigo => "Codigo",
            EServicoExecutado.Descricao => "Descricao",
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

        var lista = await connection.QueryAsync<ServicoExecutado>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken)
        );

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<ServicoExecutado>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<bool> UpdateAsync(ServicoExecutado servicoExecutado, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE ServicoExecutado
            SET
                Codigo = @Codigo,
                Descricao = @Descricao,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, servicoExecutado, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE ServicoExecutado
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id, Ativo = active }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM ServicoExecutado WHERE Codigo = @Codigo";

        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { codigo, id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }
}
