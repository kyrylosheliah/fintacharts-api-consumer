using System.Text.Json.Serialization;

namespace BackendDotnet.Models;

public class MarketBar
{
    [JsonPropertyName("t")] public DateTime T { get; set; }
    [JsonPropertyName("o")] public float O { get; set; }
    [JsonPropertyName("h")] public float H { get; set; }
    [JsonPropertyName("l")] public float L { get; set; }
    [JsonPropertyName("c")] public float C { get; set; }
    [JsonPropertyName("v")] public int V { get; set; }
}

public class MarketBarResponse
{
    [JsonPropertyName("data")] public List<MarketBar> Data { get; set; } = [];
}