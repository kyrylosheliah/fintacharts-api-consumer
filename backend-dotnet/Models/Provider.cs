namespace BackendDotnet.Models;

public class Provider
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportedMarkets { get; set; } = new();
}