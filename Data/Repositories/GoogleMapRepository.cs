namespace Data.Repositories;

public class GoogleMapRepository(IGoogleMapClient client) : IGoogleMapRepository
{
    public async Task<ApiResponse<GoogleMap>> GetAsync(string key, string? latLong = null, string? textoLivre = null, CancellationToken cancellationToken = default)
        => await client.GetAsync(latLong, textoLivre, key, cancellationToken);
}