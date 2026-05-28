namespace Business.Services;

public class DocumentoService(IDocumentoRepository documentoRepository,
    IOptions<DocumentStorageOptions> storage,
    ILogger<DocumentoService> logger,
    DbConnection connection) : IDocumentoService
{
    public async Task<List<DocumentoDto>> GetAllDocumentosByEntidadeIdAsync(Guid entidadeId, CancellationToken cancellationToken)
    {
        var documentList = await documentoRepository.GetAllByEntidadeAsync(entidadeId, cancellationToken: cancellationToken);

        var documentListResult = documentList.Select(document => (DocumentoDto)document).ToList();

        foreach(var documento in documentListResult)
            documento.Tags = await documentoRepository.GetTagsByDocumentoIdAsync(documento.Id, cancellationToken: cancellationToken);

        return documentListResult;
    }

    public async Task<Documento?> GetByIdAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => await documentoRepository.GetByIdAsync(documentoId, cancellationToken: cancellationToken);

    public async Task<Documento> AdicionarDocumentoAsync(AdicionarDocumentoRequest request, string? criadoPor, CancellationToken cancellationToken)
    {
        if (request.Arquivo is null || request.Arquivo.Length <= 0)
            throw new InvalidOperationException("Arquivo inválido.");

        var nomeOriginal = request.Nome ?? request.Arquivo.FileName;
        var extensao = Path.GetExtension(nomeOriginal);
        if (string.IsNullOrWhiteSpace(extensao)) extensao = "";

        // nome físico (no disco)
        var nomeArmazenado = $"{Guid.NewGuid():N}{extensao}";

        // caminho relativo (na tabela)
        var caminhoRelativo = DocumentoUtils.BuildCaminhoRelativo(request.EntidadeTipo, request.EntidadeId);

        var rootPath = BuildRootPath();

        // caminho físico final
        var dir = Path.Combine(rootPath, DocumentoUtils.NormalizeRel(caminhoRelativo));
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, nomeArmazenado);

        // salva e calcula hash
        string? sha256;

        await using (var target = File.Create(fullPath))
        await using (var input = request.Arquivo.OpenReadStream())
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
            MimeType = request.Arquivo.ContentType,
            TamanhoBytes = request.Arquivo.Length,
            CaminhoRelativo = caminhoRelativo,
            HashSHA256 = sha256,
            Tipo = request.Tipo,
            Descricao = request.Descricao,
            CriadoPor = criadoPor,
            Ordem = request.Ordem,
            Publico = request.Publico ?? true,
            Prioridade = request.Prioridade ?? false,
            FotoExecucao = request.FotoExecucao,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var documentoId = await documentoRepository.AddAsync(document, cancellationToken: cancellationToken);
        document.Id = documentoId;

        var vinculo = new DocumentoVinculo
        {
            DocumentoId = documentoId,
            EntidadeTipo = request.EntidadeTipo,
            EntidadeId = request.EntidadeId,
            Observacoes = request.ObservacoesVinculo ?? string.Empty,
            DataCriacao = DateTime.UtcNow
        };

        await documentoRepository.AddVinculoAsync(vinculo, cancellationToken: cancellationToken);

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

    public async Task<bool> UpdateDocumentoAsync(Guid? id, string nomeOriginal, string descricao, bool publico, bool? fotoExecucao, CancellationToken cancellationToken)
        => await documentoRepository.UpdateDocumentoAsync(id, nomeOriginal, descricao, publico, fotoExecucao, cancellationToken: cancellationToken);

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documentoRepository.GetByIdAsync(id, cancellationToken: cancellationToken);

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
    => await documentoRepository.GetTagsByDocumentoIdAsync(documentoId, cancellationToken: cancellationToken);

    public async Task SetTagsAsync(Guid documentoId, List<Guid> tagIds, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await documentoRepository.SetTagsAsync(documentoId, tagIds, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<TagDocumento>> SearchAsync(string? search, CancellationToken cancellationToken)
        => await documentoRepository.SearchAsync(search, cancellationToken: cancellationToken);

    public async Task<TagDocumento> CreateAsync(string nome, CancellationToken cancellationToken)
        => await documentoRepository.CreateAsync(nome, cancellationToken: cancellationToken);

    public async Task<bool> UpdateOrdemDocumentoAsync(List<AtualizarOrdemDocumentoRequest> atualizarOrdemListaDocumento, CancellationToken cancellationToken)
    {
        await DbUtils.EnsureOpenAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var item in atualizarOrdemListaDocumento)
            {
                var result = await documentoRepository.UpdateOrdemDocumentoAsync(item.Id, item.Ordem, transaction, cancellationToken);
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError("Erro ao atualizar ordem dos documentos.");
                    return false;
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Erro ao atualizar ordem dos documentos.");
            return false;
        }
    }
}
