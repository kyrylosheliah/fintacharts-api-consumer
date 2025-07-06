namespace BackendDotnet.Models;

public class MarketBar
{
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public float Open { get; set; }
    public float High { get; set; }
    public float Low { get; set; }
    public float Close { get; set; }
    public long Volume { get; set; }
    public string Timeframe { get; set; } = string.Empty;
}