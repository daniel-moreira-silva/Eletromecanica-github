namespace Core.Models;

public class TipoEstacao
{
    public Guid? Id { get; set; }
    public string Nome { get; set; } = null!;
    public bool Ativo { get; set; }
}
