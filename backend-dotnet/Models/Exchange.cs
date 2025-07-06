namespace BackendDotnet.Models;

public record ExchangeResponse
{
    public Dictionary<string, List<string>> Data { get; set; } = [];
}
