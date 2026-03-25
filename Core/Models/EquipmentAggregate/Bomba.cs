namespace Core.Models.EquipmentAggregate;

public class Bomba
{
    public Guid? EquipamentoId { get; set; }
    public decimal Vazao { get; set; }
    public decimal AlturaManometrica { get; set; }
    public decimal Potencia { get; set; }
}
