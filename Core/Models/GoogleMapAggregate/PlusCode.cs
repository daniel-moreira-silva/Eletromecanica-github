namespace Core.Models.GoogleMapAggregate;

public class PlusCode
{
    [JsonPropertyName("compound_code")]
    public string CompoundCode { get; set; } = default!;

    [JsonPropertyName("global_code")]
    public string GlobalCode { get; set; } = default!;
}
