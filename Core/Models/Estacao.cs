namespace Core.Models;

public class Estacao
{
    public Guid? Id { get; set; }
    public Guid TipoEstacaoId { get; set; } = default!;
    public string Nome { get; set; } = default!;
    public string? TipoEstacao { get; set; } = default!;
    public string? Observacoes { get; set; } = default!;
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Lat { get; set; }
    public string? Long { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? PontoReferencia { get; set; }
    public bool? Ativo { get; set; }
    public DateTime? DataCriacao { get; set; }
}
