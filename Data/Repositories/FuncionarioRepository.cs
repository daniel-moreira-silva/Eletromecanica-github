using Core.Models.FuncionarioAggregate;

namespace Data.Repositories;

internal class FuncionarioRepository(DbConnection connection) : IFuncionarioRepository
{
    public async Task<Guid> AddAsync(Funcionario funcionario, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Funcionario
                (Codigo, Nome, Terceirizado,
                 CargoId, SetorId, TipoFuncionarioId)
            OUTPUT INSERTED.Id
            VALUES
                (@Codigo, @Nome, @Terceirizado,
                 @CargoId, @SetorId, @TipoFuncionarioId);
        ";

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, funcionario, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Funcionario funcionario, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Funcionario
            SET
                Codigo = @Codigo,
                Nome = @Nome,
                Terceirizado = @Terceirizado,
                CargoId = @CargoId,
                SetorId = @SetorId,
                TipoFuncionarioId = @TipoFuncionarioId,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, funcionario, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Funcionario?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                F.Id,
                F.Codigo,
                F.Nome,
                F.Terceirizado,
                F.CargoId,
                F.SetorId,
                F.TipoFuncionarioId,
                F.Ativo,
                C.Descricao  AS Cargo,
                S.Descricao  AS Setor,
                TF.Descricao AS TipoFuncionario
            FROM Funcionario F
            LEFT JOIN Cargo           C  ON C.Id  = F.CargoId
            LEFT JOIN Setor           S  ON S.Id  = F.SetorId
            LEFT JOIN TipoFuncionario TF ON TF.Id = F.TipoFuncionarioId
            WHERE F.Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Funcionario>(
            new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool active, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Funcionario
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { id, Ativo = active }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<Funcionario>> PaginatedGetAsync(FuncionarioFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT
                F.Id,
                F.Codigo,
                F.Nome,
                F.Terceirizado,
                F.CargoId,
                F.SetorId,
                F.TipoFuncionarioId,
                F.Ativo,
                C.Descricao  AS Cargo,
                S.Descricao  AS Setor,
                TF.Descricao AS TipoFuncionario
            FROM Funcionario F
            LEFT JOIN Cargo           C  ON C.Id  = F.CargoId
            LEFT JOIN Setor           S  ON S.Id  = F.SetorId
            LEFT JOIN TipoFuncionario TF ON TF.Id = F.TipoFuncionarioId
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(F.Codigo) LIKE '%' + @TODOS + '%'
                    OR UPPER(F.Nome) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(F.Email, '')) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(C.Descricao, '')) LIKE '%' + @TODOS + '%'
                    OR UPPER(ISNULL(S.Descricao, '')) LIKE '%' + @TODOS + '%'
                )",
                "@TODOS", filter.Todos.Trim().ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filter.Codigo))
            builder.Where("UPPER(F.Codigo) LIKE '%' + @CODIGO + '%'",
                          "@CODIGO", filter.Codigo.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.Nome))
            builder.Where("UPPER(F.Nome) LIKE '%' + @NOME + '%'",
                          "@NOME", filter.Nome.Trim().ToUpper());

        if (filter.CargoId.HasValue)
            builder.Where("F.CargoId = @CARGOID", "@CARGOID", filter.CargoId.Value);

        if (filter.SetorId.HasValue)
            builder.Where("F.SetorId = @SETORID", "@SETORID", filter.SetorId.Value);

        if (filter.TipoFuncionarioId.HasValue)
            builder.Where("F.TipoFuncionarioId = @TIPOFUNCIONARIOID",
                          "@TIPOFUNCIONARIOID", filter.TipoFuncionarioId.Value);

        if (filter.Terceirizado.HasValue)
            builder.Where("F.Terceirizado = @TERCEIRIZADO", "@TERCEIRIZADO", filter.Terceirizado.Value);

        if (filter.Ativo.HasValue)
            builder.Where("F.Ativo = @ATIVO", "@ATIVO", filter.Ativo.Value);

        var orderBy = filter.OrdenarPor switch
        {
            EFuncionario.Codigo => "F.Codigo",
            EFuncionario.Nome => "F.Nome",
            EFuncionario.Cargo => "C.Descricao",
            EFuncionario.Setor => "S.Descricao",
            EFuncionario.TipoFuncionario => "TF.Descricao",
            _ => "F.Nome"
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

        var lista = await connection.QueryAsync<Funcionario>(
            new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<Funcionario>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<bool> ValidateCodeIsDuplicatedAsync(string codigo, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM Funcionario WHERE Codigo = @Codigo";
        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(sql, new { codigo, id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }

    public async Task<List<Funcionario>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                F.Id,
                F.Codigo,
                F.Nome,
                F.CargoId,
                F.SetorId,
                F.TipoFuncionarioId,
                F.Terceirizado,
                F.Ativo,
                C.Descricao  AS Cargo,
                S.Descricao  AS Setor,
                TF.Descricao AS TipoFuncionario
            FROM Funcionario F
            LEFT JOIN Cargo           C  ON C.Id  = F.CargoId
            LEFT JOIN Setor           S  ON S.Id  = F.SetorId
            LEFT JOIN TipoFuncionario TF ON TF.Id = F.TipoFuncionarioId
            WHERE F.Ativo = 1
            ORDER BY F.Nome;
        ";

        var result = await connection.QueryAsync<Funcionario>(
            new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }
}