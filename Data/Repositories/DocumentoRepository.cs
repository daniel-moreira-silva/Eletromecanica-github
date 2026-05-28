namespace Data.Repositories;

public class DocumentoRepository(DbConnection connection) : IDocumentoRepository
{
    public async Task<List<Documento>> GetAllByEntidadeAsync(Guid entidadeId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                d.*,
                CAST(d.TamanhoBytes AS decimal(18, 0)) AS TamanhoBytes
            FROM dbo.Documento d
            INNER JOIN dbo.DocumentoVinculo dv ON d.Id = dv.DocumentoId
            WHERE
                dv.EntidadeId = @EntidadeId
                AND d.Ativo = 1
            ORDER BY d.Ordem, d.DataCriacao ASC
        ";

        var rows = await connection.QueryAsync<Documento>(new CommandDefinition(sql, new { entidadeId }, transaction, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<Documento?> GetByIdAsync(Guid documentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            SELECT
                d.*,
                CAST(d.TamanhoBytes AS decimal(18, 0)) AS TamanhoBytes
            FROM dbo.Documento d
            WHERE d.Id = @Id;
        ";

        return await connection.QueryFirstOrDefaultAsync<Documento>(new CommandDefinition(sql, new { Id = documentoId }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddAsync(Documento documento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DECLARE @Inserted TABLE (Id UNIQUEIDENTIFIER);

            INSERT INTO dbo.Documento
            (
                NomeOriginal,
                NomeArmazenado,
                Extensao,
                MimeType,
                TamanhoBytes,
                CaminhoRelativo,
                HashSHA256,
                Tipo,
                Descricao,
                CriadoPor,
                Prioridade,
                Ordem,
                FotoExecucao,
                Publico
            )
            OUTPUT INSERTED.Id INTO @Inserted(Id)
            VALUES
            (
                @NomeOriginal,
                @NomeArmazenado,
                @Extensao,
                @MimeType,
                @TamanhoBytes,
                @CaminhoRelativo,
                @HashSHA256,
                @Tipo,
                @Descricao,
                @CriadoPor,
                @Prioridade,
                @Ordem,
                @FotoExecucao,
                @Publico
            );

            SELECT Id FROM @Inserted;
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, documento, transaction, cancellationToken: cancellationToken));
    }

    public async Task<Guid> AddVinculoAsync(DocumentoVinculo vinculo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            DECLARE @Inserted TABLE (Id UNIQUEIDENTIFIER);

            INSERT INTO dbo.DocumentoVinculo
            (
                DocumentoId,
                EntidadeTipo,
                EntidadeId,
                Observacoes
            )
            OUTPUT INSERTED.Id INTO @Inserted(Id)
            VALUES
            (
                @DocumentoId,
                @EntidadeTipo,
                @EntidadeId,
                @Observacoes
            );

            SELECT Id FROM @Inserted;
        ";

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(sql, vinculo, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateDocumentoAsync(Guid? id, string nomeOriginal, string descricao, bool publico, bool? fotoExecucao, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
            UPDATE Documento
            SET NomeOriginal = @NomeOriginal,
                Descricao = @Descricao,
                Publico = @Publico,
                FotoExecucao = @FotoExecucao
            WHERE Id = @Id;";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id, NomeOriginal = nomeOriginal, Descricao = descricao, Publico = publico, FotoExecucao = fotoExecucao }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> UpdateOrdemDocumentoAsync(Guid id, int ordem, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @" 
            UPDATE Documento 
            SET Ordem = @Ordem
            WHERE Id = @Id;";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id, Ordem = ordem }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"DELETE FROM dbo.Documento WHERE Id = @Id;";

        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { id }, transaction, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<Documento?> GetByHashAsync(string hash, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"SELECT TOP 1 * FROM dbo.Documento WHERE HashSHA256 = @hash AND Ativo = 1";

        return await connection.QueryFirstOrDefaultAsync<Documento>(new CommandDefinition(sql, new { hash }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<bool> VinculoExisteAsync(Guid documentoId, EEntidadeTipo entidadeTipo, Guid entidadeId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"SELECT 1
            FROM dbo.DocumentoVinculo
            WHERE DocumentoId = @documentoId
                AND EntidadeTipo = @entidadeTipo
                AND EntidadeId = @entidadeId";

        var exists = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { documentoId, entidadeTipo, entidadeId }, transaction, cancellationToken: cancellationToken));
        return exists.HasValue;
    }

    public async Task<List<TagDocumento>> SearchAsync(string? search, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
          SELECT TOP 50 Id, Nome, NomeNormalizado, Ativo
          FROM dbo.TagDocumento
          WHERE Ativo = 1
            AND (@search IS NULL OR Nome LIKE '%' + @search + '%')
          ORDER BY Nome;
        ";

        var rows = await connection.QueryAsync<TagDocumento>(new CommandDefinition(sql, new { search }, transaction, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<List<TagDocumento>> GetTagsByDocumentoIdAsync(Guid documentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string sql = @"
          SELECT t.Id, t.Nome, t.NomeNormalizado, t.Ativo
          FROM dbo.DocumentoTag dt
          INNER JOIN dbo.TagDocumento t ON t.Id = dt.TagId
          WHERE dt.DocumentoId = @documentoId AND t.Ativo = 1
          ORDER BY t.Nome;
        ";

        var rows = await connection.QueryAsync<TagDocumento>(new CommandDefinition(sql, new { documentoId }, transaction, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task SetTagsAsync(Guid documentoId, IReadOnlyCollection<Guid> tagIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        const string del = @"DELETE FROM dbo.DocumentoTag WHERE DocumentoId = @documentoId;";
        await connection.ExecuteAsync(new CommandDefinition(del, new { documentoId }, transaction, cancellationToken: cancellationToken));

        if (tagIds.Count == 0) return;

        const string ins = @"INSERT INTO dbo.DocumentoTag (DocumentoId, TagId) VALUES (@documentoId, @tagId);";

        foreach (var tagId in tagIds.Distinct())
            await connection.ExecuteAsync(new CommandDefinition(ins, new { documentoId, tagId }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<TagDocumento> CreateAsync(string nome, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        var nomeFormatado = (nome ?? "").Trim();

        var normalizado = nomeFormatado.ToUpperInvariant().Replace(" ", "");

        const string sql = @"
          IF EXISTS (SELECT 1 FROM dbo.TagDocumento WHERE NomeNormalizado = @normalizado)
          BEGIN
             SELECT TOP 1 Id, Nome, NomeNormalizado, Ativo
             FROM dbo.TagDocumento
             WHERE NomeNormalizado = @normalizado;
          END
          ELSE
          BEGIN
             DECLARE @Inserted TABLE (Id UNIQUEIDENTIFIER);

             INSERT INTO dbo.TagDocumento (Nome, NomeNormalizado, Ativo)
             OUTPUT INSERTED.Id INTO @Inserted(Id)
             VALUES (@nome, @normalizado, 1);

             SELECT t.Id, t.Nome, t.NomeNormalizado, t.Ativo
             FROM dbo.TagDocumento t
             INNER JOIN @Inserted i ON i.Id = t.Id;
          END
        ";

        return await connection.QuerySingleAsync<TagDocumento>(new CommandDefinition(sql, new { nome = nomeFormatado, normalizado }, transaction, cancellationToken: cancellationToken));
    }
}
