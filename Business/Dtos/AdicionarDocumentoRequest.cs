namespace Business.Dtos;

public class AdicionarDocumentoRequest
{
    public Guid EntidadeId { get; set; }
    public EEntidadeTipo EntidadeTipo { get; set; }
    public ETipoDocumento? Tipo { get; set; }
    public string? Descricao { get; set; }
    public string? ObservacoesVinculo { get; set; }

    public IFormFile Arquivo { get; set; } = default!;
}
