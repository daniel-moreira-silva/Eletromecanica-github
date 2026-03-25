namespace Core.Models.OrdemServicoAggregate;

public class StatusOrdemServico
{
    public Guid? Id { get; set; }
    public string Descricao { get; set; } = default!;
    public bool? Ativo { get; set; }
}
