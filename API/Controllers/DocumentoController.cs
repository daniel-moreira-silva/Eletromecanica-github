namespace API.Controllers;

[ApiController]
[Route("documentos")]
public class DocumentoController(IDocumentoService service, IOptions<DocumentStorageOptions> storage, ILogger<DocumentoController> logger) : BaseController(logger)
{
    [HttpGet("listarDocumentosPorEntidade")]
    public async Task<IActionResult> GetAllAnexosByContratoAsync([FromQuery] Guid entidadeId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllDocumentosByEntidadeIdAsync(entidadeId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter documentos");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO, ex.Message));
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var doc = await service.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (doc is null || doc.Ativo == false) return NotFound();

        var safeRelative = (doc.CaminhoRelativo ?? string.Empty)
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(storage.Value.RootPath, safeRelative, doc.NomeArmazenado);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("Arquivo não encontrado no storage.");

        var stream = System.IO.File.OpenRead(fullPath);

        var mime = string.IsNullOrWhiteSpace(doc.MimeType)
            ? MediaTypeNames.Application.Octet
            : doc.MimeType;

        return File(stream, mime, doc.NomeOriginal);
    }

    [HttpGet("{id}/view")]
    public async Task<IActionResult> View(Guid id, CancellationToken cancellationToken)
    {
        var document = await service.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (document is null || !document.Ativo) return NotFound();

        var safeRelative = (document.CaminhoRelativo ?? string.Empty)
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(storage.Value.RootPath, safeRelative, document.NomeArmazenado);
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var stream = System.IO.File.OpenRead(fullPath);
        var mime = string.IsNullOrWhiteSpace(document.MimeType) ? "application/octet-stream" : document.MimeType;

        var fileName = ControllerUtils.SanitizeFileNameForHeader(document.NomeOriginal);

        var cd = new ContentDispositionHeaderValue("inline");
        cd.SetHttpFileName(fileName);
        Response.Headers[HeaderNames.ContentDisposition] = cd.ToString();

        return File(stream, mime);
    }

    [HttpPost("adicionarDocumento")]
    public async Task<ActionResult<DocumentoDto>> Upload([FromForm] AdicionarDocumentoRequest request, CancellationToken cancellationToken)
    {
        var criadoPor = Request.Headers["Usuario"].ToString();

        var document = await service.AdicionarDocumentoAsync(
            request.EntidadeId,
            request.EntidadeTipo,
            request.Arquivo,
            request.Tipo,
            request.Descricao,
            request.ObservacoesVinculo,
            criadoPor,
            cancellationToken
        );

        return Ok(document);
    }

    [HttpPut("atualizarDocumento")]
    public async Task<IActionResult> UpdateDocumentoAsync([FromBody] DocumentoDto documento, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateDocumentoAsync(documento.Id, documento.NomeOriginal, cancellationToken);

            if (result)
                return Ok(new SuccessMessage("Documento atualizado com sucesso.", null));
            else
                return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO, "Erro ao atualizar documento."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar documento.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO, ex.Message));
        }
    }

    [HttpDelete("excluirDocumento")]
    public async Task<IActionResult> DeleteDocumentoAsync([FromQuery] Guid id, CancellationToken cancelationToken)
    {
        try
        {
            var result = await service.DeleteByIdAsync(id, cancelationToken);

            if (result)
                return Ok(new SuccessMessage("Documento excluído com sucesso.", null));
            else
                return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO, "Erro ao excluir documento."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao excluir documento.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO, ex.Message));
        }
    }

    [HttpGet("{id}/tags")]
    public async Task<IActionResult> GetTags(Guid id, CancellationToken ct)
    {
        var tags = await service.GetTagsByDocumentoIdAsync(id, ct);
        return Ok(tags);
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> SetTags(Guid id, [FromBody] SetDocumentoTagRequest request, CancellationToken ct)
    {
        await service.SetTagsAsync(id, request.TagIds, ct);
        return Ok(new SuccessMessage("Tags atualizadas com sucesso.", null));
    }

    [HttpGet("tags")]
    public async Task<IActionResult> Search([FromQuery] string? search, CancellationToken cancelationToken)
    {
        var tags = await service.SearchAsync(search, cancelationToken);
        return Ok(tags);
    }

    [HttpPost("tags")]
    public async Task<IActionResult> Create([FromBody] SetTagRequest request, CancellationToken cancelationToken)
    {
        var tag = await service.CreateAsync(request.Nome, cancelationToken);
        return Ok(tag);
    }
}