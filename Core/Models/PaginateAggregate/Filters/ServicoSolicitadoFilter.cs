namespace Core.Models.PaginateAggregate.Filters
{
    public class ServicoSolicitadoFilter : Filter<EServicoSolicitado>
    {
        public string? Codigo { get; set; }
        public string? Descricao { get; set; }
    }
}
