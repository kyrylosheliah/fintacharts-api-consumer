using System.Data;
using BackendDotnet.Models;
using Npgsql;

namespace BackendDotnet.Data;

public class TimescaleDBClient(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("TimescaleDB:ConnectionString") ?? "";

    public async Task<bool> PutAssetData(List<MarketBar> data)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var cmd = new NpgsqlCommand
            {
                Connection = conn,
                Transaction = tx,
                CommandText = @"
                    INSERT INTO market_data (timestamp, symbol, open, high, low, close, volume, timeframe)
                    VALUES (@timestamp, @symbol, @open, @high, @low, @close, @volume, @timeframe);"
            };

            cmd.Parameters.Add(new("@timestamp", DbType.DateTime));
            cmd.Parameters.Add(new("@symbol", DbType.String));
            cmd.Parameters.Add(new("@open", DbType.Decimal));
            cmd.Parameters.Add(new("@high", DbType.Decimal));
            cmd.Parameters.Add(new("@low", DbType.Decimal));
            cmd.Parameters.Add(new("@close", DbType.Decimal));
            cmd.Parameters.Add(new("@volume", DbType.Decimal));
            cmd.Parameters.Add(new("@timeframe", DbType.String));

            foreach (var bar in data)
            {
                cmd.Parameters["@timestamp"].Value = bar.Timestamp;
                cmd.Parameters["@symbol"].Value = bar.Symbol;
                cmd.Parameters["@open"].Value = bar.Open;
                cmd.Parameters["@high"].Value = bar.High;
                cmd.Parameters["@low"].Value = bar.Low;
                cmd.Parameters["@close"].Value = bar.Close;
                cmd.Parameters["@volume"].Value = bar.Volume;
                cmd.Parameters["@timeframe"].Value = bar.Timeframe;

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            await tx.RollbackAsync();
            return false;
        }
    }

    public async Task<List<MarketBar>> GetAssetData(string asset, string timeRange)
    {
        var bars = new List<MarketBar>();
        var query = @"
            SELECT timestamp, timeframe, open, high, low, close, volume
            FROM market_data
            WHERE symbol = @symbol
              AND timestamp >= NOW() - @timeRange::interval
            ORDER BY timestamp DESC;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@symbol", asset);
        cmd.Parameters.AddWithValue("@timeRange", timeRange);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bars.Add(new MarketBar
            {
                Timestamp = reader.GetDateTime(0),
                Timeframe = reader.GetString(1),
                Open = reader.GetDecimal(2),
                High = reader.GetDecimal(3),
                Low = reader.GetDecimal(4),
                Close = reader.GetDecimal(5),
                Volume = (uint)reader.GetDecimal(6),
            });
        }

        return bars;
    }
}
