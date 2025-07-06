using System.Text.Json;
using BackendDotnet.Models;
using QuestDB;

namespace BackendDotnet.Data;

public class QuestDBClient(IConfiguration configuration)
{
    private readonly HttpClient _http = new();
    private readonly string _connection_string = configuration.GetConnectionString("QuestDB:ConnectionString") ?? "";

    public async Task<bool> EnsureSchema()
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS assets (
                timestamp TIMESTAMP,
                timeframe STRING
                symbol SYMBOL,
                open FLOAT,
                high FLOAT,
                low FLOAT,
                close FLOAT,
                volume INT,
            ) TIMESTAMP(timestamp);
        ";
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("query", sql)
        ]);
        var response = await _http.PostAsync($"{_connection_string}/exec", content);
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> PutAssetValue(MarketBar bar)
    {
        var sender = Sender.New(_connection_string);
        try
        {
            await sender.Table("assets")
                .Symbol("symbol", bar.Symbol)
                .Column("open", bar.Open)
                .Column("high", bar.High)
                .Column("low", bar.Low)
                .Column("close", bar.Close)
                .Column("volume", bar.Volume)
                .Column("timeframe", bar.Timeframe)
                .AtAsync(bar.Timestamp);
            await sender.SendAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PutAssetData(List<MarketBar> data)
    {
        var sender = Sender.New(_connection_string);
        try
        {
            foreach (var bar in data)
            {
                await sender.Table("assets")
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

    public async Task<AssetData> GetRecentAssetValue(string symbol)
    {
        string query = $"SELECT timestamp, timeframe, symbol, open, high, low, close, volume FROM assets WHERE symbol = {symbol} ORDER BY timestamp DESC LIMIT 1";
        string url = $"{_connection_string}?query={Uri.EscapeDataString(query)}";
        var res = await _http.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            var bar = new MarketBar()
            {
                Timestamp = DateTime.Parse(row[0].GetString() ?? ""),
                Timeframe = row[1].GetString() ?? "",
                Symbol = row[2].GetString() ?? "",
                Open = (float)row[3].GetDouble(),
                High = (float)row[4].GetDecimal(),
                Low = (float)row[5].GetDecimal(),
                Close = (float)row[6].GetDecimal(),
                Volume = row[7].GetUInt32(),
            };
            return new() { data = [ bar ] };
        }
        return new() { data = [] };
    }

    public async Task<AssetData> GetAssetData(string symbol, string timeRange)
    {
        string query = $"SELECT timestamp, timeframe, symbol, open, high, low, close, volume FROM assets WHERE symbol = {symbol} AND timestamp IN '{timeRange}' ORDER BY timestamp DESC";
        string url = $"{_connection_string}?query={Uri.EscapeDataString(query)}";
        var res = await _http.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        var bars = new List<MarketBar>();
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            bars.Add(new()
            {
                Timestamp = DateTime.Parse(row[0].GetString() ?? ""),
                Timeframe = row[1].GetString() ?? "",
                Symbol = row[2].GetString() ?? "",
                Open = (float)row[3].GetDouble(),
                High = (float)row[4].GetDecimal(),
                Low = (float)row[5].GetDecimal(),
                Close = (float)row[6].GetDecimal(),
                Volume = row[7].GetUInt32(),
            });
        }

        return new() { data = bars };
    }
}

public record AssetData
{
    public List<MarketBar> data = [];
}