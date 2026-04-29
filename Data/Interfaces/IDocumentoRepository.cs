namespace Data.Interfaces;

public interface IDocumentoRepository
{
    Task<List<Documento>> GetAllByEntidadeAsync(Guid entidadeId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Documento?> GetByIdAsync(Guid documentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Guid> AddAsync(Documento documento, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Guid> AddVinculoAsync(DocumentoVinculo vinculo, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateDocumentoAsync(Guid? id, string nomeOriginal, string descricao, bool publico, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrdemDocumentoAsync(Guid id, int ordem, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(Guid id, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<Documento?> GetByHashAsync(string hash, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> VinculoExisteAsync(Guid documentoId, EEntidadeTipo entidadeTipo, Guid entidadeId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<TagDocumento>> SearchAsync(string? search, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<List<TagDocumento>> GetTagsByDocumentoIdAsync(Guid documentoId, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task SetTagsAsync(Guid documentoId, IReadOnlyCollection<Guid> tagIds, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<TagDocumento> CreateAsync(string nome, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}