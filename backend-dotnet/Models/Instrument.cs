namespace BackendDotnet.Models;

public class Instrument
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal TickSize { get; set; }
}