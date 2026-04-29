namespace Business.Dtos;

public class AtualizarDocumentoRequest
{
    public Guid Id { get; set; }
    public string NomeOriginal { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public bool Publico { get; set; } = default!;
}