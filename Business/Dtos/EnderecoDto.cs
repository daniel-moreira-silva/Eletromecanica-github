namespace Business.Dtos;

public class EnderecoDto
{
    public string Rua { get; init; } = default!;
    public string Numero { get; init; } = default!;
    public string Bairro { get; init; } = default!;
    public string Cidade { get; init; } = default!;
    public string Estado { get; init; } = default!;
    public string Cep { get; init; } = default!;
    public string Pais { get; init; } = default!;
    public string EnderecoFormatado { get; init; } = default!;
    public string Lat { get; init; } = default!;
    public string Long { get; init; } = default!;
}
