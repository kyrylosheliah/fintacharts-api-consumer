using System.Text.Json.Serialization;

namespace BackendDotnet.Models;

public class WebSocketMessage
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("instrumentId")] public string InstrumentId { get; set; } = "";
    [JsonPropertyName("provider")] public string Provider { get; set; } = "";
    [JsonPropertyName("subscribe")] public bool Subscribe { get; set; } = true;
    [JsonPropertyName("kinds")] public List<string> Kinds { get; set; } = ["ask", "bid", "last"];
}
