namespace Business.Dtos;

public class EquipamentoComponenteDto
{
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public string? Tag { get; set; }

    public static implicit operator EquipamentoComponenteDto(Equipamento equipamento)
    {
        return new EquipamentoComponenteDto
        {
            Id = equipamento.Id!.Value,
            Nome = equipamento.Nome,
            Tag = equipamento.Tag
        };
    }
}
