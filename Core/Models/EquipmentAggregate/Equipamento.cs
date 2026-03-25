namespace Core.Models.EquipmentAggregate;

public class Equipamento
{
    public Guid? Id { get; set; }
    public Guid EstacaoId { get; set; }
    public Guid TipoEquipamentoId { get; set; }
    public Guid? EquipamentoPrincipalId { get; set; }
    public string? TipoEquipamento { get; set; }
    public string? Estacao { get; set; }
    public string? EquipamentoPrincipal { get; set; }
    public string Nome { get; set; } = default!;
    public string? Tag { get; set; }
    public string? Fabricante { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
