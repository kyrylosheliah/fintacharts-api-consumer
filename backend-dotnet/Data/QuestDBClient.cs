using System.Text.Json;
using BackendDotnet.Models;
using QuestDB;

namespace BackendDotnet.Data;

public class QuestDBClient(IConfiguration configuration)
{
    private readonly HttpClient _http = new();
    private readonly string _connection_string = configuration.GetConnectionString("QuestDB:ConnectionString") ?? "";

    public async Task<bool> PutAssetData(List<MarketBar> data)
    {
        var sender = Sender.New(_connection_string);
        try
        {
            foreach (var bar in data)
            {
                await sender.Table("MarketData")
                    .Symbol("symbol", bar.Symbol)
                    .Column("open", bar.Open)
                    .Column("high", bar.High)
                    .Column("low", bar.Low)
                    .Column("close", bar.Close)
                    .Column("volume", bar.Volume)
                    .Column("timeframe", bar.Timeframe)
                    .AtAsync(bar.Timestamp);
            }
            await sender.SendAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<AssetData> GetAssetData(string asset, string timeRange)
    {
        string query = $"SELECT time, o, h, l, c, v FROM {asset} WHERE time IN '{timeRange}' ORDER BY time DESC";
        string url = $"{_connection_string}?query={Uri.EscapeDataString(query)}";
        var res = await _http.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        var bars = new List<AssetBar>();
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            bars.Add(new AssetBar
            {
                t = DateTime.Parse(row[0].GetString() ?? ""),
                o = (float)row[1].GetDouble(),
                h = (float)row[2].GetDecimal(),
                l = (float)row[3].GetDecimal(),
                c = (float)row[4].GetDecimal(),
                v = row[5].GetUInt32()
            });
        }

        return new(){ data = bars };
    }
}