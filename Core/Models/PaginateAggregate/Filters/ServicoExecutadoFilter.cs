namespace Core.Models.PaginateAggregate.Filters;

public class ServicoExecutadoFilter : Filter<EServicoExecutado>
{
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
}
