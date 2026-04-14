namespace Core.Models.PaginateAggregate.Filters;

public class OrdemServicoFilter : Filter<EOrdemServico>
{
    public string? Numero { get; set; }
    //---
    public IEnumerable<Guid>? StatusId { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public IEnumerable<Guid>? RegiaoId { get; set; }
    public IEnumerable<Guid>? AgendamentoId { get; set; }
    public IEnumerable<Guid>? ServicoSolicitadoId { get; set; }
    public DateTime? DataSolicitacaoInicio { get; set; }
    public DateTime? DataSolicitacaoFim { get; set; }
    public DateTime? DataAgendamentoInicio { get; set; }
    public DateTime? DataAgendamentoFim { get; set; }
    public DateTime? DataDespachoInicio { get; set; }
    public DateTime? DataDespachoFim { get; set; }
    public DateTime? DataFinalizacaoInicio { get; set; }
    public DateTime? DataFinalizacaoFim { get; set; }
    public DateTime? DataParalisacaoInicio { get; set; }
    public DateTime? DataParalisacaoFim { get; set; }
    public DateTime? DataDespachoProgramadoInicio { get; set; }
    public DateTime? DataDespachoProgramadoFim { get; set; }
    public DateTime? DataCancelamentoInicio { get; set; }
    public DateTime? DataCancelamentoFim { get; set; }
    public string? RegiaoAgendamento { get; set; }
    public IEnumerable<Guid>? EquipeId { get; set; }
    public IEnumerable<Guid>? ServicoExecutadoId { get; set; }
    public IEnumerable<Guid>? MotivoCancelamentoId { get; set; }
    public string? PontoReferencia { get; set; }
    public string? NumeroDocumento { get; set; }
    public bool? IsAgendada { get; set; }
    public bool? IsProgramada { get; set; }
}
