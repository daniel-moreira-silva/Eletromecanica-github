namespace Core.Models.GoogleMapAggregate;

public class AddressComponent
{
    [JsonPropertyName("long_name")]
    public string LongName { get; set; } = default!;

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; } = default!;

    [JsonPropertyName("types")]
    public List<string> Types { get; set; } = default!;
}
