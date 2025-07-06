using BackendDotnet.Data;
using BackendDotnet.Models;
using BackendDotnet.Services;

namespace BackendDotnet.Endpoints;

public class MarketBarEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/symbol")
            .WithTags("Symbol Market Bars");
        group
            .MapGet("/health", GetHealth)
            .WithSummary("Get the operational status for the endpoint group");
        group
            .MapGet("/assets", GetAssets)
            .WithSummary("Get a filtered instrument list");
        group
            .MapGet("/bars", GetSymbolBars)
            .WithSummary("Get the aggregated bar data for the specified symbol");
        group
            .MapGet("/bar", GetSymbolBar)
            .WithSummary("Get the most recent value point for the specified symbol");
        group
            .MapGet("/test/token", TestGetToken)
            .WithSummary("Test token state");
    }

    public static async Task<IResult> GetHealth()
    {
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    public static async Task<IResult> GetAssets(
        string? symbol,
        int? page,
        int? size,
        string? provider,
        AssetCache assetCache
    )
    {
        var instruments = await assetCache.GetInstruments(symbol, page, size, provider);
        return Results.Ok(instruments);
    }

    private static async Task<IResult> GetSymbolBars(
        string symbol,
        string timeRange,
        QuestDBClient dbClient
    )
    {
        var result = await dbClient.GetAssetData(symbol, timeRange);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetSymbolBar(
        string symbol,
        SymbolBarCache cache,
        QuestDBClient dbClient
    )
    {
        if (cache.TryGet(symbol, out MarketBar? cacheResult))
        {
            var result = new AssetData() { data = [cacheResult] };
            return Results.Ok(result);
        }
        Serilog.Log.Information("GetSymbolBar(): cache miss");
        var dbResult = await dbClient.GetRecentAssetValue(symbol);
        return Results.Ok(dbResult);
    }

    public static async Task<IResult> TestGetToken(AuthTokenService tokenService)
    {
        var token = await tokenService.GetAccessTokenAsync();
        return Results.Ok(new { token, timestamp = DateTime.UtcNow });
    }
}