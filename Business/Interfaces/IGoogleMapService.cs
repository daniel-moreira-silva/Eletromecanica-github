namespace Business.Interfaces;

public interface IGoogleMapService
{
    Task<EnderecoDto> GetAsync(string? latLong = null, string? textoLivre = null, bool retentativa = false, CancellationToken cancellationToken = default);
}