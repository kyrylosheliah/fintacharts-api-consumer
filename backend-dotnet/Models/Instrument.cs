namespace BackendDotnet.Models;

public record InstrumentResponse
{
    public InstrumentPaging Paging = new();
    public List<Instrument> Data = [];
}

public record Instrument
{
    public string Id = "";
    public string Symbol = "";
    public string Kind = "";
    public string Description = "";
    public float TickSize;
    public string Currency = "";
    public string BaseCurrency = "";
    public Dictionary<string, InstrumentMapping> Mappings = [];
    public InstrumentProfile profile = new();
}

public record InstrumentMapping
{
    public string Symbol = "";
    public string Exchange = "";
    public int DefaultOrderSize;
    public int MaxOrderSize;
    public MappingTradingHours TradingHours = new();
}

public record MappingTradingHours {
    public string RegularStart = "";
    public string RegularEnd = "";
    public string ElectronicStart = "";
    public string ElectronicEnd = "";
}

public record InstrumentProfile {
    public string name = "";
    public object gics = new();
}

public record InstrumentPaging {
    public int Page;
    public int Pages;
    public int Items;
}
