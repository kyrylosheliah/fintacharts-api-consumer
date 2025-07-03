using System.Text.Json;

namespace BackendDotnet.Data;

public class OHLCVBar
{
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}

public class QuestDBClient
{
    private readonly HttpClient _http = new();
    private readonly string _baseUrl = "http://localhost:9000/exec";

    public async Task<List<OHLCVBar>> GetOHLCVAsync(string symbol)
    {
        string query = $"SELECT ts, open, high, low, close, volume FROM ohlcv WHERE symbol = '{symbol}' ORDER BY ts DESC LIMIT 100";
        string url = $"{_baseUrl}?query={Uri.EscapeDataString(query)}";
        var res = await _http.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        var bars = new List<OHLCVBar>();
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            bars.Add(new OHLCVBar
            {
                Timestamp = DateTime.Parse(row[0].GetString()),
                Open = row[1].GetDecimal(),
                High = row[2].GetDecimal(),
                Low = row[3].GetDecimal(),
                Close = row[4].GetDecimal(),
                Volume = row[5].GetDecimal()
            });
        }

        return bars;
    }
}