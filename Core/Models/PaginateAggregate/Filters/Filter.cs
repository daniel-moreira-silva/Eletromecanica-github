namespace Core.Models.PaginateAggregate.Filters;

public class Filter<T>
{
    public string? Todos { get; set; } = default!;
    public bool? Ativo { get; set; }
    public T OrdenarPor { get; set; } = default!;
    public EAscDesc Ordem { get; set; }
    public int Pagina { get; set; }
    public int ItensPagina { get; set; }
}
