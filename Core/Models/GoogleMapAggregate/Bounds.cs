namespace Core.Models.GoogleMapAggregate;

public class Bounds
{
    [JsonPropertyName("northeast")]
    public Northeast Northeast { get; set; } = default!;

    [JsonPropertyName("southwest")]
    public Southwest Southwest { get; set; } = default!;
}
