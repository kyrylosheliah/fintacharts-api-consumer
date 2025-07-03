namespace BackendDotnet.Models;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public object Data { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}