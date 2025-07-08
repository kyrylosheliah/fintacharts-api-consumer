using System.Text.Json.Serialization;

namespace BackendDotnet.Models;

public class InstrumentResponse
{
    [JsonPropertyName("paging")] public InstrumentPaging Paging { get; set; } = new();
    [JsonPropertyName("data")] public List<Instrument> Data { get; set; } = [];
}

public class Instrument
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("symbol")] public string Symbol { get; set; } = "";
    [JsonPropertyName("kind")] public string Kind { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("tickSize")] public float TickSize { get; set; }
    [JsonPropertyName("currency")] public string Currency { get; set; } = "";
    [JsonPropertyName("baseCurrency")] public string BaseCurrency { get; set; } = "";
    [JsonPropertyName("mappings")] public Dictionary<string, InstrumentMapping> Mappings { get; set; } = [];
    [JsonPropertyName("profile")] public InstrumentProfile Profile { get; set; } = new();
}

public class InstrumentMapping
{
    [JsonPropertyName("symbol")] public string Symbol { get; set; } = "";
    [JsonPropertyName("exchange")] public string Exchange { get; set; } = "";
    [JsonPropertyName("defaultOrderSize")] public int DefaultOrderSize { get; set; }
    [JsonPropertyName("maxOrderSize")] public int MaxOrderSize { get; set; }
    [JsonPropertyName("tradingHours")] public MappingTradingHours TradingHours { get; set; } = new();
}

public class MappingTradingHours
{
    [JsonPropertyName("regularStart")] public string RegularStart { get; set; } = "";
    [JsonPropertyName("regularEnd")] public string RegularEnd { get; set; } = "";
    [JsonPropertyName("electronicStart")] public string ElectronicStart { get; set; } = "";
    [JsonPropertyName("electronicEnd")] public string ElectronicEnd { get; set; } = "";
}

public class InstrumentProfile
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("gics")] public object Gics { get; set; } = new();
}

public class InstrumentPaging
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pages")] public int Pages { get; set; }
    [JsonPropertyName("items")] public int Items { get; set; }
}
