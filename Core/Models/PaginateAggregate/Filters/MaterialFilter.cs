namespace Core.Models.PaginateAggregate.Filters
{
    public class MaterialFilter : Filter<EMaterial>
    {
        public string? Codigo { get; set; }
        public string? Descricao { get; set; }
    }
}
