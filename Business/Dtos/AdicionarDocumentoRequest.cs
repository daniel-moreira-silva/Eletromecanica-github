namespace Business.Dtos;

public class AdicionarDocumentoRequest
{
    public Guid EntidadeId { get; set; }
    public EEntidadeTipo EntidadeTipo { get; set; }
    public ETipoDocumento? Tipo { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public string? ObservacoesVinculo { get; set; }
    public bool? Publico { get; set; }
    public bool? Prioridade { get; set; }
    public bool? FotoExecucao { get; set; }
    public int? Ordem { get; set; }
    public IFormFile Arquivo { get; set; } = default!;
}
