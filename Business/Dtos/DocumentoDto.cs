namespace Business.Dtos;

public class DocumentoDto
{
    public Guid Id { get; set; }
    public string NomeOriginal { get; set; } = default!;
    public string Extensao { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public decimal TamanhoBytes { get; set; }
    public DateTime DataCriacao { get; set; }
    public List<TagDocumento> Tags { get; set; } = [];
    public static implicit operator DocumentoDto(Documento document)
    {
        var mime = string.IsNullOrWhiteSpace(document.MimeType) ? "application/octet-stream" : document.MimeType;

        return new DocumentoDto
        {
            Id = document.Id!.Value,
            NomeOriginal = document.NomeOriginal,
            Extensao = document.Extensao,
            MimeType = mime,
            TamanhoBytes = document.TamanhoBytes,
            DataCriacao = document.DataCriacao,
        };
    }
}