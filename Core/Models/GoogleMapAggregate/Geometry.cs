namespace Core.Models.GoogleMapAggregate;

public class Geometry
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = default!;

    [JsonPropertyName("location_type")]
    public string LocationType { get; set; } = default!;

    [JsonPropertyName("viewport")]
    public Viewport Viewport { get; set; } = default!;

    [JsonPropertyName("bounds")]
    public Bounds Bounds { get; set; } = default!;
}
