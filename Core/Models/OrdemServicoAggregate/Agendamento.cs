namespace Core.Models.OrdemServicoAggregate;

public class Agendamento
{
    public Guid? Id { get; set; }
    public string? Descricao { get; set; }
    public ETipoRecorrencia TipoRecorrencia { get; set; }
    public int? DiaSemana { get; set; }
    public int? DiaMes { get; set; }
    public int? DiasPrevios { get; set; }
    public int? LimiteAgendamento { get; set; }
    public bool AgendamentoFixo { get; set; }
    public DateTime? DataAgendamentoFixo { get; set; }
    public bool Ativo { get; set; }
}
