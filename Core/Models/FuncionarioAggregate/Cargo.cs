namespace Core.Models.FuncionarioAggregate;

public class Cargo
{
    public Guid? Id { get; set; }
    public string Descricao { get; set; } = default!;
    public bool? Ativo { get; set; }
}