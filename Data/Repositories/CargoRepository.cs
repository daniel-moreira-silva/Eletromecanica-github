using Core.Models.FuncionarioAggregate;

namespace Data.Repositories;

internal class CargoRepository(DbConnection connection) : ICargoRepository
{
    public async Task<Guid> AddAsync(Cargo cargo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Cargo (Descricao)
            OUTPUT INSERTED.Id
            VALUES (@Descricao);
        ";

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, cargo, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Cargo cargo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Cargo
            SET
                Descricao = @Descricao,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, cargo, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Cargo?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Descricao, Ativo
            FROM Cargo
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Cargo>(
            new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Cargo
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { id, Ativo = active }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<Cargo>> PaginatedGetAsync(CargoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT Id, Descricao, Ativo
            FROM Cargo
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where("UPPER(Descricao) LIKE '%' + @TODOS + '%'",
                "@TODOS", filter.Todos.Trim().ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filter.Descricao))
            builder.Where("UPPER(Descricao) LIKE '%' + @DESCRICAO + '%'",
                          "@DESCRICAO", filter.Descricao.Trim().ToUpper());

        if (filter.Ativo.HasValue)
            builder.Where("Ativo = @ATIVO", "@ATIVO", filter.Ativo.Value);

        var orderBy = filter.OrdenarPor switch
        {
            ECargo.Descricao => "Descricao",
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

        var lista = await connection.QueryAsync<Cargo>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<Cargo>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<bool> ValidateDescriptionIsDuplicatedAsync(string descricao, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM Cargo WHERE UPPER(Descricao) = UPPER(@Descricao)";
        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(sql, new { descricao, id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }

    public async Task<List<Cargo>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Descricao, Ativo
            FROM Cargo
            WHERE Ativo = 1
            ORDER BY Descricao;
        ";

        var result = await connection.QueryAsync<Cargo>(
            new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }
}