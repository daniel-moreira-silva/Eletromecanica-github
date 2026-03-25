namespace Core.Models.EquipmentAggregate;

public class Motor
{
    public Guid? EquipamentoId { get; set; }
    public decimal Potencia { get; set; }
    public int Tensao { get; set; }
    public int Rpm { get; set; }
}
