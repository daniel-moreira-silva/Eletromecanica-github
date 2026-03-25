namespace Core.Models.EquipmentAggregate;

public class TipoEquipamento
{
    public Guid? Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Categoria { get; set; } = default!;
    public ENivelTipoEquipamento Nivel { get; set; }
    public bool Ativo { get; set; }
}