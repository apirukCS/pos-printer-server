using System.Text.Json.Serialization;

public class DestinationPrinter
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("ipTarget")]
    public string IpTarget { get; set; }
}

public class CashDrawerModel
{
    [JsonPropertyName("destination")]
    public DestinationPrinter Destination { get; set; }

    [JsonPropertyName("docType")]
    public string DocType { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }
}
