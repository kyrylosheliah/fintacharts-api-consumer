namespace BackendDotnet.Models;

public class HistoricalBarsRequest
{
    public string Symbol { get; set; } = string.Empty;
    public int? Count { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Timeframe { get; set; } = "1m";
}