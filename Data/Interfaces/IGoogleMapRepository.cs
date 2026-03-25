namespace Data.Interfaces;

public interface IGoogleMapRepository
{
    Task<ApiResponse<GoogleMap>> GetAsync(string key, string? latLong = null, string? textoLivre = null, CancellationToken cancellationToken = default);
}