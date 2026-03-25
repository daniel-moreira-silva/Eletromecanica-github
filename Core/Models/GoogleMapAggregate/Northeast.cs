namespace Core.Models.GoogleMapAggregate;

public class Northeast
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}
