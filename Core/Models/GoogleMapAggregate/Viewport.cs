namespace Core.Models.GoogleMapAggregate;

public class Viewport
{
    [JsonPropertyName("northeast")]
    public Northeast Northeast { get; set; } = default!;

    [JsonPropertyName("southwest")]
    public Southwest Southwest { get; set; } = default!;
}