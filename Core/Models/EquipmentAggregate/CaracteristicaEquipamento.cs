namespace Core.Models.EquipmentAggregate;

public class CaracteristicaEquipamento
{
    public Guid? Id { get; set; }
    public Guid? EquipamentoId { get; set; }
    public string? Nome { get; set; } = null!;
    public string? Valor { get; set; } = null!;
    public string? UnidadeMedida { get; set; } = null!;
    public ETipoValorCaracteristicaEquipamento TipoValor { get; set; }
    public DateTime DataCriacao { get; set; }
}
