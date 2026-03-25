namespace Core.Models.EquipmentAggregate;

public class RegraPreventivaServicoSolicitado
{
    public Guid RegraPreventivaId { get; set; }
    public Guid ServicoSolicitadoId { get; set; }

    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
}