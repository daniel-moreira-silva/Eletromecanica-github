namespace Business.Interfaces;

public interface IDocumentoService
{
    Task<List<DocumentoDto>> GetAllDocumentosByEntidadeIdAsync(Guid entidadeId, CancellationToken cancellationToken);
    Task<Documento?> GetByIdAsync(Guid documentoId, CancellationToken cancellationToken = default);
    Task<Documento> AdicionarDocumentoAsync(AdicionarDocumentoRequest request, string? criadoPor, CancellationToken cancellationToken);
    Task<bool> UpdateDocumentoAsync(Guid? id, string nomeOriginal, string descricao, bool publico, CancellationToken cancellationToken);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TagDocumento>> GetTagsByDocumentoIdAsync(Guid documentoId, CancellationToken cancellationToken);
    Task SetTagsAsync(Guid documentoId, List<Guid> tagIds, CancellationToken cancellationToken);
    Task<List<TagDocumento>> SearchAsync(string? search, CancellationToken cancellationToken);
    Task<TagDocumento> CreateAsync(string nome, CancellationToken cancellationToken);
    Task<bool> UpdateOrdemDocumentoAsync(List<AtualizarOrdemDocumentoRequest> atualizarOrdemListaDocumento, CancellationToken cancellationToken);
}
