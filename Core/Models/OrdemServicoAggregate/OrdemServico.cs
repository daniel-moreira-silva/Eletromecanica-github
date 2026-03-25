namespace Core.Models.OrdemServicoAggregate;

public class OrdemServico
{
    public Guid? Id { get; set; }
    public string? Codigo { get; set; }
    public int? Numero { get; set; }
    public int? SubOS { get; set; }
    public int? Ano { get; set; }
    public Guid EstacaoId { get; set; }
    public Guid? AgendamentoId { get; set; }
    public string? Agendamento { get; set; } = default!;
    public Guid StatusId { get; set; }
    public string? Status { get; set; } = default!;
    public Guid? MotivoCancelamentoId { get; set; }
    public string? MotivoCancelamento { get; set; } = default!;
    public Guid? RegiaoId { get; set; }
    public string? Regiao { get; set; } = default!;
    public ETipoOS TipoOS { get; set; }
    public EPrioridadeOS Prioridade { get; set; }
    public string? Email { get; set; }
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public string? NumeroDocumento { get; set; }
    public DateTime? DataAgendamento { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public DateTime? DataDespacho { get; set; }
    public DateTime? DataDespachoProgramado { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataParalisacao { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataInicioExecucao { get; set; }
    public string? Observacao { get; set; }
    public bool IsAgendada { get; set; }
    public List<OrdemServicoServicoSolicitado>? ServicosSolicitados { get; set; }
    public List<OrdemServicoEquipamento>? Equipamentos { get; set; }
}