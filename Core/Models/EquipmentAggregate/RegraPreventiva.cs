namespace Core.Models.EquipmentAggregate;

public class RegraPreventiva
{
    public Guid? Id { get; set; }
    public Guid EquipamentoId { get; set; }
    public string Nome { get; set; } = default!;
    public int Intervalo { get; set; }
    public EUnidadePeriodoPreventivo UnidadePeriodo { get; set; }
    public EStatusRegraPreventiva? Status { get; set; } = EStatusRegraPreventiva.AguardandoProcessamento;
    public DateTime DataInicio { get; set; }
    public DateTime? ProximoProcessamento { get; set; }
    public DateTime? UltimoProcessamento { get; set; }
    public EPrioridade Prioridade { get; set; }
    public bool Ativo { get; set; }
    public DateTime? DataCriacao { get; set; }
    public List<RegraPreventivaServicoSolicitado> ServicosSolicitados { get; set; } = [];
}