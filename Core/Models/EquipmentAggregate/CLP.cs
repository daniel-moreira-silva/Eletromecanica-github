namespace Core.Models.EquipmentAggregate;

public class CLP
{
    public Guid? EquipamentoId { get; set; }
    public string Marca { get; set; } = null!;
    public string? Firmware { get; set; }
}
