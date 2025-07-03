namespace BackendDotnet.Models;

public class MarketQuoteMessage
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal Last { get; set; }
    public long BidSize { get; set; }
    public long AskSize { get; set; }
    public long Volume { get; set; }
    public long Timestamp { get; set; }
}