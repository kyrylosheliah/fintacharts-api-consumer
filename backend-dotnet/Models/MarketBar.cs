namespace BackendDotnet.Models;

public class MarketBar
{
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public string Timeframe { get; set; } = string.Empty;
}