namespace Core.Models.EquipmentAggregate;

public class MedidorVazao
{
    public Guid? EquipamentoId { get; set; }
    public string? Fabricante { get; set; }
    public string? ModeloConversor { get; set; }
    public string? ModeloSensor { get; set; }
    public decimal Diametro { get; set; }
    public decimal? FatorK { get; set; }
    public decimal? EscalaMaxima { get; set; }
}