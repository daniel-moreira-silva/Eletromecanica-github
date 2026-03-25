namespace Core.Models.OrdemServicoAggregate;

public class ServicoExecutado
{
    public Guid? Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public bool? Ativo { get; set; }
}
