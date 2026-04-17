namespace Core.Models.FuncionarioAggregate;

public class Funcionario
{
    public Guid? Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Nome { get; set; } = default!;
    public bool Terceirizado { get; set; }
    public string TerceirizadoDescricao { get { return Terceirizado ? "Sim" : "Não"; } }
    public string? Empresa { get; set; }
    public Guid CargoId { get; set; }
    public Guid SetorId { get; set; }
    public Guid TipoFuncionarioId { get; set; }
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public string? TipoFuncionario { get; set; }
    public bool? Ativo { get; set; }
}