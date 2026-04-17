namespace Core.Models.PaginateAggregate.Filters;

public class FuncionarioFilter : Filter<EFuncionario>
{
    public string? Codigo { get; set; }
    public string? Nome { get; set; }
    public Guid? CargoId { get; set; }
    public Guid? SetorId { get; set; }
    public Guid? TipoFuncionarioId { get; set; }
    public bool? Terceirizado { get; set; }
}