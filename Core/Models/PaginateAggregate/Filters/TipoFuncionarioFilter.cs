namespace Core.Models.PaginateAggregate.Filters;

public class TipoFuncionarioFilter : Filter<ETipoFuncionario>
{
    public string? Descricao { get; set; }
}