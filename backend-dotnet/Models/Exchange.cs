namespace BackendDotnet.Models;

public class Exchange
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}

public class ExchangeResponse
{
    public Dictionary<string, List<string>> Data { get; set; } = [];
}
