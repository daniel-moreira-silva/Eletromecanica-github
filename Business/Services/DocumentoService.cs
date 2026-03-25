namespace Business.Services;

public class DocumentoService(IDocumentoRepository documentoRepository, IOptions<DocumentStorageOptions> storage, DbConnection connection) : IDocumentoService
{
    public async Task<List<DocumentoDto>> GetAllDocumentosByEntidadeIdAsync(Guid entidadeId, CancellationToken cancellationToken)
    {
        var documentList = await documentoRepository.GetAllByEntidadeAsync(entidadeId, null, cancellationToken);

        var documentListResult = documentList.Select(document => (DocumentoDto)document).ToList();

        foreach(var documento in documentListResult)
            documento.Tags = await documentoRepository.GetTagsByDocumentoIdAsync(documento.Id, null, cancellationToken);

        return documentListResult;
    }

    public async Task<Documento?> GetByIdAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => await documentoRepository.GetByIdAsync(documentoId, null, cancellationToken);

    public async Task<Documento> AdicionarDocumentoAsync(
        Guid entidadeId,
        EEntidadeTipo entidadeTipo,
        IFormFile arquivo,
        ETipoDocumento? tipo,
        string? descricao,
        string? observacoesVinculo,
        string? criadoPor,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length <= 0)
            throw new InvalidOperationException("Arquivo inválido.");

        var nomeOriginal = arquivo.FileName;
        var extensao = Path.GetExtension(nomeOriginal);
        if (string.IsNullOrWhiteSpace(extensao)) extensao = "";

        // nome físico (no disco)
        var nomeArmazenado = $"{Guid.NewGuid():N}{extensao}";

        // caminho relativo (na tabela)
        var caminhoRelativo = DocumentoUtils.BuildCaminhoRelativo(entidadeTipo, entidadeId);

        var rootPath = BuildRootPath();

        // caminho físico final
        var dir = Path.Combine(rootPath, DocumentoUtils.NormalizeRel(caminhoRelativo));
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, nomeArmazenado);

        // salva e calcula hash
        string? sha256;

        await using (var target = File.Create(fullPath))
        await using (var input = arquivo.OpenReadStream())
        using (var hasher = SHA256.Create())
        using (var crypto = new CryptoStream(target, hasher, CryptoStreamMode.Write, leaveOpen: true))
        {
            await input.CopyToAsync(crypto, cancellationToken);

            // finaliza o hash
            crypto.FlushFinalBlock();

            sha256 = Convert.ToHexString(hasher.Hash!).ToLowerInvariant();
        }

        var document = new Documento
        {
            NomeOriginal = nomeOriginal,
            NomeArmazenado = nomeArmazenado,
            Extensao = extensao,
            MimeType = arquivo.ContentType,
            TamanhoBytes = arquivo.Length,
            CaminhoRelativo = caminhoRelativo,
            HashSHA256 = sha256,
            Tipo = tipo,
            Descricao = descricao,
            CriadoPor = criadoPor,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var documentoId = await documentoRepository.AddAsync(document, null, cancellationToken);
        document.Id = documentoId;

        var vinculo = new DocumentoVinculo
        {
            DocumentoId = documentoId,
            EntidadeTipo = entidadeTipo,
            EntidadeId = entidadeId,
            Observacoes = observacoesVinculo ?? string.Empty,
            DataCriacao = DateTime.UtcNow
        };

        await documentoRepository.AddVinculoAsync(vinculo, null, cancellationToken);

        return document;
    }

    private string BuildRootPath()
    {
        var configured = storage?.Value?.RootPath;

        if (!string.IsNullOrWhiteSpace(configured))
        {
            try
            {
                var full = Path.GetFullPath(configured);
                Directory.CreateDirectory(full);

                return full;
            }
            catch
            {
                return AppContext.BaseDirectory;
            }
        }

        return AppContext.BaseDirectory;
    }

    public async Task<bool> UpdateDocumentoAsync(Guid? id, string nomeOriginal, CancellationToken cancellationToken) 
        => await documentoRepository.UpdateDocumentoAsync(id, nomeOriginal, null, cancellationToken);

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documentoRepository.GetByIdAsync(id, null, cancellationToken);

        if (document is null) return false;

        var safeRelative = (document.CaminhoRelativo ?? string.Empty)
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(storage.Value.RootPath, safeRelative, document.NomeArmazenado);

        await DbUtils.EnsureOpenAsync(connection, cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            var result = await documentoRepository.DeleteByIdAsync(id, transaction, cancellationToken);

            if (result)
            {
                await transaction.CommitAsync(cancellationToken);
                return true;
            }

            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
    }

    public async Task<List<TagDocumento>> GetTagsByDocumentoIdAsync(Guid documentoId, CancellationToken cancellationToken)
    => await documentoRepository.GetTagsByDocumentoIdAsync(documentoId, null, cancellationToken);

    public async Task SetTagsAsync(Guid documentoId, List<Guid> tagIds, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await documentoRepository.SetTagsAsync(documentoId, tagIds, tx, cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<TagDocumento>> SearchAsync(string? search, CancellationToken cancellationToken)
        => await documentoRepository.SearchAsync(search, null, cancellationToken);

    public async Task<TagDocumento> CreateAsync(string nome, CancellationToken cancellationToken)
        => await documentoRepository.CreateAsync(nome, null, cancellationToken);
}
