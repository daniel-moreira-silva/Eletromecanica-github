namespace Core.Models.PaginateAggregate;

public class ListaPaginada<T>
{
    public List<T> Lista { get; set; } = [];
    public int Paginas { get; set; }
    public int TotalItens { get; set; }
}