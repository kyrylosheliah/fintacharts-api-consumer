using System.Text.Json;
using BackendDotnet.Models;
using QuestDB;
using Serilog;

namespace BackendDotnet.Data;

public class QuestDBClient
{
    private readonly HttpClient _httpClient;
    private readonly string _connection_string;
    private readonly string _rest_connection_string;

    public QuestDBClient(
        IConfiguration configuration,
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        var protocol = configuration["QuestDB:Protocol"] ?? "";
        var url = configuration["QuestDB:Url"] ?? "";
        var username = configuration["QuestDB:Username"] ?? "";
        var password = configuration["QuestDB:Password"] ?? "";
        _connection_string = $"{protocol}::addr={url};username={username};password={password};";
        _rest_connection_string = $"{protocol}://{url}";
    }

    public async Task<bool> EnsureSchema()
    {
        string url = $"{_rest_connection_string}/exec?query=SHOW TABLES";
        HttpResponseMessage selectResponse = await _httpClient.GetAsync(url);
        string json = await selectResponse.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        if (!(doc.RootElement.TryGetProperty("dataset", out JsonElement dataset) && dataset.GetArrayLength() > 0))
        {
            return false;
        }
        foreach (var row in dataset.EnumerateArray())
        {
            if (string.Equals(row[0].GetString(), "assets", StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("The DB table exists");
                return true;
            }
        }
        Log.Information("Creating a table");
        var sql = @"
            CREATE TABLE assets (
                timestamp TIMESTAMP,
                symbol SYMBOL,
                o FLOAT,
                h FLOAT,
                l FLOAT,
                c FLOAT,
                v INT
            ) TIMESTAMP(timestamp);
        ";
        var createResponse = await _httpClient.GetAsync($"{_rest_connection_string}/exec?query={Uri.EscapeDataString(sql)}");
        createResponse.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> PutAssetValue(string symbol, MarketBar bar)
    {
        var sender = Sender.New(_connection_string);
        try
        {
            await sender.Table("assets")
                .Symbol("symbol", symbol)
                .Column("o", bar.O)
                .Column("h", bar.H)
                .Column("l", bar.L)
                .Column("c", bar.C)
                .Column("v", bar.V)
                .AtAsync(bar.T);
            await sender.SendAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PutAssetData(string symbol, List<MarketBar> data)
    {
        var sender = Sender.New(_connection_string);
        try
        {
            foreach (var bar in data)
            {
                await sender.Table("assets")
                    .Symbol("symbol", symbol)
                    .Column("o", bar.O)
                    .Column("h", bar.H)
                    .Column("l", bar.L)
                    .Column("c", bar.C)
                    .Column("v", bar.V)
                    .AtAsync(bar.T);
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
        string query = $"SELECT timestamp, o, h, l, c, v FROM assets WHERE symbol = {symbol} ORDER BY timestamp DESC LIMIT 1";
        string url = $"{_connection_string}?query={Uri.EscapeDataString(query)}";
        var res = await _httpClient.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            var bar = new MarketBar()
            {
                T = DateTime.Parse(row[0].GetString() ?? ""),
                O = (float)row[1].GetDouble(),
                H = (float)row[2].GetDecimal(),
                L = (float)row[3].GetDecimal(),
                C = (float)row[4].GetDecimal(),
                V = row[5].GetInt32(),
            };
            return new() { data = [ bar ] };
        }
        return new() { data = [] };
    }

    public async Task<AssetData> GetAssetData(string symbol, string timeRange)
    {
        string query = $"SELECT timestamp, o, h, l, c, v FROM assets WHERE symbol = {symbol} AND timestamp IN '{timeRange}' ORDER BY timestamp DESC";
        string url = $"{_connection_string}?query={Uri.EscapeDataString(query)}";
        var res = await _httpClient.GetStringAsync(url);
        var parsed = JsonDocument.Parse(res);
        var bars = new List<MarketBar>();
        foreach (var row in parsed.RootElement.GetProperty("dataset").EnumerateArray())
        {
            bars.Add(new()
            {
                T = DateTime.Parse(row[0].GetString() ?? ""),
                O = (float)row[1].GetDouble(),
                H = (float)row[2].GetDecimal(),
                L = (float)row[3].GetDecimal(),
                C = (float)row[4].GetDecimal(),
                V = row[5].GetInt32(),
            });
        }

        return new() { data = bars };
    }
}

public record AssetData
{
    public List<MarketBar> data = [];
}