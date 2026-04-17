namespace Core.Models.PaginateAggregate.Filters;

public class CargoFilter : Filter<ECargo>
{
    public string? Descricao { get; set; }
}