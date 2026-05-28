namespace Core.Models.EquipmentAggregate;

public class Material
{
    public Guid? Id { get; set; }
    public string? Codigo { get; set; }
    public string Descricao { get; set; } = default!;
    public string? UnidadeMedida { get; set; }
    public bool? Ativo { get; set; }
}
