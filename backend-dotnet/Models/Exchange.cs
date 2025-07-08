using System.Text.Json.Serialization;

namespace BackendDotnet.Models;

public record ExchangeResponse
{
    [JsonPropertyName("data")] public Dictionary<string, List<string>> Data { get; set; } = [];
}
