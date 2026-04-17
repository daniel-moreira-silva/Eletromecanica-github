namespace Core.Models.FuncionarioAggregate;

public class Setor
{
    public Guid? Id { get; set; }
    public string Descricao { get; set; } = default!;
    public bool? Ativo { get; set; }
}