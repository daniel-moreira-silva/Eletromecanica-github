namespace Core.Models.PaginateAggregate.Filters;

public class EstacaoFilter : Filter<EEstacao>
{
    public string? Nome { get; set; }
    public string? TipoEstacaoId { get; set; }
    public string? Localizacao { get; set; }
}
