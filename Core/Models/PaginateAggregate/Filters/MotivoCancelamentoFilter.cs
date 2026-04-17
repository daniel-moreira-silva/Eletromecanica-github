namespace Core.Models.PaginateAggregate.Filters;

public class MotivoCancelamentoFilter : Filter<EMotivoCancelamento>
{
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
}