namespace Core.Models.OrdemServicoAggregate;

public class Regiao
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = null!;
    public bool Ativo { get; set; }
}