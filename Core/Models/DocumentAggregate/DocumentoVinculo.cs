namespace Core.Models.DocumentAggregate;

public class DocumentoVinculo
{
    public Guid? Id { get; set; }
    public Guid DocumentoId { get; set; }
    public EEntidadeTipo EntidadeTipo { get; set; }
    public string DescricaoEntidadeTipo { get { return EntidadeTipo.ToString(); } }
    public Guid EntidadeId { get; set; }
    public string Observacoes { get; set; } = default!;
    public DateTime DataCriacao { get; set; }
}
