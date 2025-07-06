namespace BackendDotnet.Models;

public class Provider
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> SupportedMarkets { get; set; } = [];
}

public class ProviderResponse
{
    public List<string> Data { get; set; } = [];
}
