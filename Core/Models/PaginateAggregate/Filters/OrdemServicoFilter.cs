namespace Core.Models.PaginateAggregate.Filters;

public class OrdemServicoFilter : Filter<EOrdemServico>
{
    public string? Numero { get; set; }
}
