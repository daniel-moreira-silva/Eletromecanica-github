namespace Core.Models.GoogleMapAggregate;

public class NavigationPoint
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = default!;
}