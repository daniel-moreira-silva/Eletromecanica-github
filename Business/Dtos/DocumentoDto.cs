namespace Business.Dtos;

public class DocumentoDto
{
    public Guid Id { get; set; }
    public string NomeOriginal { get; set; } = default!;
    public string Extensao { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public decimal TamanhoBytes { get; set; }
    public DateTime DataCriacao { get; set; }
    public string DataCriacaoFormatada { get { return DataCriacao.ToBrazilianFormat(); } }
    public string? Descricao { get; set; }
    public bool? Publico { get; set; }
    public int? Ordem { get; set; }
    public bool? FotoExecucao { get; set; }
    public string? Caminho { get; set; }
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
            Publico = document.Publico,
            Ordem = document.Ordem,
            FotoExecucao = document.FotoExecucao,
            Caminho = document.CaminhoRelativo,
            Descricao = document.Descricao,
        };
    }

    public static implicit operator Documento(DocumentoDto dto)
    {
        return new Documento
        {
            Id = dto.Id,
            NomeOriginal = dto.NomeOriginal,
            Extensao = dto.Extensao,
            MimeType = dto.MimeType,
            TamanhoBytes = (long)dto.TamanhoBytes,
            DataCriacao = dto.DataCriacao,
            Publico = dto.Publico ?? true,
            Ordem = dto.Ordem,
            FotoExecucao = dto.FotoExecucao,
            CaminhoRelativo = dto.Caminho ?? string.Empty,
            Descricao = dto.Descricao,
        };
    }
}