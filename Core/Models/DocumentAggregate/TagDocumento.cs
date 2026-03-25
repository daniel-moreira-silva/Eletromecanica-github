namespace Core.Models.DocumentAggregate;

public class TagDocumento
{
    public Guid? Id { get; set; }
    public string Nome { get; set; } = default!;
    public string NomeNormalizado { get { return Nome.Trim().ToUpperInvariant().Replace(" ", ""); } }
    public bool Ativo { get; set; }
}