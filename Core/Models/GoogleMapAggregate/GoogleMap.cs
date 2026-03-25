namespace Core.Models.GoogleMapAggregate;

public class GoogleMap
{
    [JsonPropertyName("plus_code")]
    public PlusCode PlusCode { get; set; } = default!;

    [JsonPropertyName("results")]
    public List<Result> Results { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}