namespace Core.Models.FuncionarioAggregate;

public class TipoFuncionario
{
    public Guid? Id { get; set; }
    public string Descricao { get; set; } = default!;
    public bool? Ativo { get; set; }
}