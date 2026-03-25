namespace Core.Models.DocumentAggregate;

public class Documento
{
    public Guid? Id { get; set; }
    public string NomeOriginal { get; set; } = default!;
    public string NomeArmazenado { get; set; } = default!;
    public string Extensao { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public long TamanhoBytes { get; set; } = default!;
    public string CaminhoRelativo { get; set; } = default!;
    public string? HashSHA256 { get; set; }
    public ETipoDocumento? Tipo { get; set; }
    public string? DescricaoTipo { get { return Tipo.HasValue ? Tipo.Value.ToString() : null; } }
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public string? CriadoPor { get; set; }
    public bool Ativo { get; set; }
}
