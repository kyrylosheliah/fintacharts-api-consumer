namespace BackendDotnet.Models;

public class Instrument
{
    public string Id { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Description { get; set; } = "";
    public string Currency { get; set; } = "";
    public string BaseCurrency { get; set; } = "";
    public Dictionary<string, InstrumentMapping> Mappings { get; set; } = [];
}

public class InstrumentMapping
{
    public string Symbol { get; set; } = "";
    public string Exchange { get; set; } = "";
}
