namespace Core.Models.GoogleMapAggregate;

public class Result
{
    [JsonPropertyName("address_components")]
    public List<AddressComponent> AddressComponents { get; set; } = default!;

    [JsonPropertyName("formatted_address")]
    public string FormattedAddress { get; set; } = default!;

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; } = default!;

    [JsonPropertyName("navigation_points")]
    public List<NavigationPoint> NavigationPoints { get; set; } = default!;

    [JsonPropertyName("place_id")]
    public string PlaceId { get; set; } = default!;

    [JsonPropertyName("types")]
    public List<string> Types { get; set; } = default!;

    [JsonPropertyName("plus_code")]
    public PlusCode PlusCode { get; set; } = default!;
}
