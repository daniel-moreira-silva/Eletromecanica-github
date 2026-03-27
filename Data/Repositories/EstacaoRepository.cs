namespace Data.Repositories;

public class EstacaoRepository(DbConnection connection) : IEstacaoRepository
{
    public async Task<Guid> AddAsync(Estacao estacao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            INSERT INTO Estacao (
                Nome, 
                TipoEstacaoId, 
                Observacoes, 
                Endereco,
                Bairro,
                Lat,
                Long,
                Numero,
                Complemento,
                PontoReferencia)
            OUTPUT INSERTED.Id
            VALUES (
                @Nome, 
                @TipoEstacaoId, 
                @Observacoes, 
                @Endereco,
                @Bairro,
                @Lat,
                @Long,
                @Numero,
                @Complemento,
                @PontoReferencia);
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, estacao, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Estacao estacao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Estacao
            SET
                Nome = @Nome,
                TipoEstacaoId = @TipoEstacaoId,
                Observacoes = @Observacoes,
                Endereco = @Endereco,
                Bairro = @Bairro,
                Lat = @Lat,
                Long = @Long,
                Numero = @Numero,
                Complemento = @Complemento,
                PontoReferencia = @PontoReferencia,
                Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, estacao, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Estacao?> GetByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Estacao
            WHERE Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Estacao>(new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateStatusAsync(Guid id, bool ativo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Estacao
            SET Ativo = @Ativo
            WHERE Id = @Id;
        ";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Ativo = ativo }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<ListaPaginada<Estacao>> PaginatedGetAsync(EstacaoFilter filter, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var baseQuery = @"
            SELECT E.*, TE.NOME AS TipoEstacao
            FROM Estacao E
            JOIN TipoEstacao TE ON E.TipoEstacaoId = TE.Id
        ";

        var builder = new SqlQueryBuilder();

        if (!string.IsNullOrWhiteSpace(filter.Todos))
        {
            builder.Where(@"
                (
                    UPPER(E.NOME) LIKE '%' + @TODOS + '%'
                    OR TE.NOME LIKE '%' + @TODOS + '%'
                    OR E.LOCALIZACAO LIKE '%' + @TODOS + '%'
                )",
                "@TODOS", filter.Todos.Trim().ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filter.Nome))
            builder.Where("UPPER(E.NOME) LIKE '%' + @NOME + '%'",
                          "@NOME", filter.Nome.ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.TipoEstacaoId))
            builder.Where("TE.ID = @TIPOESTACAOID",
                          "@TIPOESTACAOID", filter.TipoEstacaoId);

        if (!string.IsNullOrWhiteSpace(filter.Localizacao))
            builder.Where("UPPER(E.LOCALIZACAO) LIKE '%' + @LOCALIZACAO + '%'",
                          "@LOCALIZACAO", filter.Localizacao.ToUpper());

        if (filter.Ativo.HasValue)
            builder.Where("E.ATIVO = @ATIVO", "@ATIVO", filter.Ativo);

        var orderBy = filter.OrdenarPor switch
        {
            EEstacao.Nome => "E.NOME",
            EEstacao.TipoEstacao => "TE.NOME",
            EEstacao.Localizacao => "E.LOCALIZACAO",
            _ => "E.NOME"
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

        var lista = await connection.QueryAsync<Estacao>(new CommandDefinition(query, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(queryCount, builder.Parameters, transaction, cancellationToken: cancellationToken));

        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)filter.ItensPagina));

        return new ListaPaginada<Estacao>
        {
            Lista = lista.AsList(),
            Paginas = paginas,
            TotalItens = total
        };
    }

    public async Task<List<Estacao>> GetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT *
            FROM Estacao
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<Estacao>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<List<TipoEstacao>> TipoEstacaoGetAllAsync(IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT Id, Nome, Ativo
            FROM TipoEstacao
            WHERE Ativo = 1;
        ";

        var result = await connection.QueryAsync<TipoEstacao>(new CommandDefinition(sql, transaction, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<bool> ValidateNameIsDuplicatedAsync(string nome, Guid? id = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        string sql = @"SELECT 1 FROM Estacao WHERE Nome = @Nome";

        if (id != null) sql += " AND Id <> @Id";

        var result = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { Nome = nome, Id = id }, transaction, cancellationToken: cancellationToken));
        return result is not null;
    }

    public async Task<Estacao?> GetByEquipamentoId(Guid equipamentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT E.*
            FROM Estacao E
            JOIN Equipamento EQ ON EQ.EstacaoId = E.Id
            WHERE EQ.Id = @EquipamentoId;
        ";

        return await connection.QueryFirstOrDefaultAsync<Estacao>(new CommandDefinition(sql, new { EquipamentoId = equipamentoId }, transaction, cancellationToken: cancellationToken));
    }
}
