namespace Data.Repositories;

public class MotivoCancelamentoRepository(DbConnection connection) : IMotivoCancelamentoRepository
{
    public async Task<Guid> AddAsync(MotivoCancelamento motivo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO MotivoCancelamento (Codigo, Descricao)
            OUTPUT INSERTED.Id
            VALUES (@Codigo, @Descricao);
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, motivo, transaction, cancellationToken: cancellationToken));
    }

    public async Task<List<MotivoCancelamento>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Codigo, Descricao, Ativo
            FROM MotivoCancelamento
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<MotivoCancelamento>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<MotivoCancelamento?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Codigo, Descricao, Ativo
            FROM MotivoCancelamento
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<MotivoCancelamento>(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(MotivoCancelamento motivo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE MotivoCancelamento
            SET Codigo = @Codigo, Descricao = @Descricao
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, motivo, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = "UPDATE MotivoCancelamento SET Ativo = @Ativo WHERE Id = @Id;";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Ativo = ativo }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT COUNT(1) FROM MotivoCancelamento
            WHERE Codigo = @Codigo AND (@Id IS NULL OR Id <> @Id);
        ";

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { Codigo = codigo, Id = id }, transaction, cancellationToken: cancellationToken));
        return count > 0;
    }

    public async Task<ListaPaginada<MotivoCancelamento>> PaginatedGetAsync(MotivoCancelamentoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT Id, Codigo, Descricao, Ativo
            FROM MotivoCancelamento
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
            builder.Where(@"(UPPER(Codigo) LIKE '%' + @TODOS + '%' OR UPPER(Descricao) LIKE '%' + @TODOS + '%')",
                "@TODOS", filter.Todos.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Codigo))
            builder.Where("UPPER(Codigo) LIKE '%' + @CODIGO + '%'", "@CODIGO", filter.Codigo.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Descricao))
            builder.Where("UPPER(Descricao) LIKE '%' + @DESCRICAO + '%'", "@DESCRICAO", filter.Descricao.Trim().ToUpper());

        if (filter.Ativo.HasValue)
            builder.Where("Ativo = @ATIVO", "@ATIVO", filter.Ativo.Value);

        var orderBy = filter.OrdenarPor switch
        {
            EMotivoCancelamento.Codigo => "Codigo",
            EMotivoCancelamento.Descricao => "Descricao",
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

        var lista = await connection.QueryAsync<MotivoCancelamento>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken));

        return new ListaPaginada<MotivoCancelamento>
        {
            Lista = lista.AsList(),
            Paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina)),
            TotalItens = total
        };
    }
}