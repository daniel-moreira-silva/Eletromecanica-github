namespace Data.Clients;

public interface IGoogleMapClient
{
    [Get("")]
    Task<ApiResponse<GoogleMap>> GetAsync([AliasAs("latlng")] string? latlng, [AliasAs("address")] string? address, [AliasAs("key")] string key, CancellationToken cancellationToken);
}