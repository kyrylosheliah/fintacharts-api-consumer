using System.Text.Json.Serialization;

namespace BackendDotnet.Models;

public class ProviderResponse
{
    [JsonPropertyName("data")] public List<string> Data { get; set; } = [];
}
