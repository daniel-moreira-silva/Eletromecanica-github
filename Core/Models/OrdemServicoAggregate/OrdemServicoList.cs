using Core.Utils;

namespace Core.Models.OrdemServicoAggregate;

public class OrdemServicoList
{
    public Guid? Id { get; set; }
    public string? Codigo { get; set; }
    public int? Numero { get; set; }
    public int? SubOS { get; set; }
    public int? Ano { get; set; }
    public Guid EstacaoId { get; set; }
    public string Estacao { get; set; } = default!;
    public string Endereco { get; set; } = default!;
    public Guid? AgendamentoId { get; set; }
    public string? Agendamento { get; set; } = default!;
    public Guid StatusId { get; set; }
    public string? Status { get; set; } = default!;
    public Guid? MotivoCancelamentoId { get; set; }
    public string? MotivoCancelamento { get; set; } = default!;
    public Guid? RegiaoId { get; set; }
    public string? Regiao { get; set; } = default!;
    public ETipoOS TipoOS { get; set; }
    public string TipoOSDescricao => TipoOS.GetDescription();
    public EPrioridadeOS Prioridade { get; set; }
    public string PrioridadeDescricao => Prioridade.GetDescription();
    public string? Email { get; set; }
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public string? NumeroDocumento { get; set; }
    public DateTime? DataAgendamento { get; set; }
    public string? DataAgendamentoFormatada { get { return DataAgendamento.ToBrazilianFormat(); } }
    public DateTime? DataCancelamento { get; set; }
    public string? DataCancelamentoFormatada { get { return DataCancelamento.ToBrazilianFormat(); } }
    public DateTime? DataDespacho { get; set; }
    public string? DataDespachoFormatada { get { return DataDespacho.ToBrazilianFormat(); } }
    public DateTime? DataDespachoProgramado { get; set; }
    public string? DataDespachoProgramadoFormatada { get { return DataDespachoProgramado.ToBrazilianFormat(); } }
    public DateTime? DataFinalizacao { get; set; }
    public string? DataFinalizacaoFormatada { get { return DataFinalizacao.ToBrazilianFormat(); } }
    public DateTime? DataParalisacao { get; set; }
    public string? DataParalisacaoFormatada { get { return DataParalisacao.ToBrazilianFormat(); } }
    public DateTime DataSolicitacao { get; set; }
    public string DataSolicitacaoFormatada { get { return DataSolicitacao.ToBrazilianFormat(); } }
    public DateTime? DataInicioExecucao { get; set; }
    public string? DataInicioExecucaoFormatada { get { return DataInicioExecucao.ToBrazilianFormat(); } }
    //public DateTime? DataPrevista { get; set; }
    //public string? DataPrevistaFormatada { get; set; }
    public decimal? CustoTotal { get; set; }   // custo acumulado
    public string? Observacao { get; set; }
    public bool IsAgendada { get; set; }
    public bool IsProgramada { get; set; }
    public string? Situacao { get { return IsAgendada == true ? "Agendada" : IsProgramada == true ? "Programada" : "Emergencial"; } }
}