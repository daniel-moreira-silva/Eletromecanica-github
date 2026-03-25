namespace Core.Models.PaginateAggregate.Filters;

public class EquipamentoFilter : Filter<EEquipamento>
{
    public string? TipoEquipamento { get; set; }
    public string? Estacao { get; set; }
    public string? Tag { get; set; }
    public string? Fabricante { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }
    public string? Nome { get; set; }
    public bool? SomentePrincipais { get; set; }
    public bool? SomenteComponentes { get; set; }
    public ENivelTipoEquipamento? NivelTipoEquipamento { get; set; }
}
