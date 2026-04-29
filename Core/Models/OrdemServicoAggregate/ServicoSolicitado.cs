namespace Core.Models.OrdemServicoAggregate;

public class ServicoSolicitado
{
    public Guid? Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public EPrioridade Prioridade { get; set; }
    public int Sla { get; set; }
    public bool? Ativo { get; set; }
}
