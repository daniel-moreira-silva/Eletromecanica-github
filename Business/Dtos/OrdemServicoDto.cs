namespace Business.Dtos;

public class OrdemServicoDto
{
    public Guid? Id { get; set; }
    public string? Codigo { get; set; }
    public int? Numero { get; set; }
    public int? SubOS { get; set; }
    public int? Ano { get; set; }
    public Guid? OrdemServicoPaiId { get; set; }
    public Guid EstacaoId { get; set; }
    public Estacao? Estacao { get; set; } = default!;
    public Guid? AgendamentoId { get; set; }
    public string? Agendamento { get; set; } = default!;
    public Guid StatusId { get; set; }
    public string? Status { get; set; } = default!;
    public Guid? MotivoCancelamentoId { get; set; }
    public string? MotivoCancelamento { get; set; } = default!;
    public Guid? RegiaoId { get; set; }
    public string? Regiao { get; set; } = default!;
    public ETipoOS TipoOS { get; set; }
    public EPrioridade Prioridade { get; set; }
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
    public decimal? CustoTotal { get; set; }
    public string? Observacao { get; set; }
    public bool IsAgendada { get; set; }
    public List<ServicoSolicitado>? ServicosSolicitados { get; set; }
    public List<Equipamento>? Equipamentos { get; set; }

    public static implicit operator OrdemServicoDto?(OrdemServico? ordemServico)
    {
        if (ordemServico == null) return null;
        return new OrdemServicoDto
        {
            Id = ordemServico.Id,
            Codigo = ordemServico.Codigo,
            Numero = ordemServico.Numero,
            SubOS = ordemServico.SubOS,
            Ano = ordemServico.Ano,
            OrdemServicoPaiId = ordemServico.OrdemServicoPaiId,
            EstacaoId = ordemServico.EstacaoId,
            AgendamentoId = ordemServico.AgendamentoId,
            Agendamento = ordemServico.Agendamento,
            StatusId = ordemServico.StatusId,
            Status = ordemServico.Status,
            MotivoCancelamentoId = ordemServico.MotivoCancelamentoId,
            MotivoCancelamento = ordemServico.MotivoCancelamento,
            RegiaoId = ordemServico.RegiaoId,
            Regiao = ordemServico.Regiao,
            TipoOS = ordemServico.TipoOS,
            Prioridade = ordemServico.Prioridade,
            Email = ordemServico.Email,
            Nome = ordemServico.Nome,
            Telefone = ordemServico.Telefone,
            NumeroDocumento = ordemServico.NumeroDocumento,
            DataAgendamento = ordemServico.DataAgendamento,
            DataCancelamento = ordemServico.DataCancelamento,
            DataDespacho = ordemServico.DataDespacho,
            DataDespachoProgramado = ordemServico.DataDespachoProgramado,
            DataFinalizacao = ordemServico.DataFinalizacao,
            DataParalisacao = ordemServico.DataParalisacao,
            DataSolicitacao = ordemServico.DataSolicitacao,
            DataInicioExecucao = ordemServico.DataInicioExecucao,
            CustoTotal = ordemServico.CustoTotal,
            Observacao = ordemServico.Observacao,
            IsAgendada = ordemServico.IsAgendada
        };
    }
}
