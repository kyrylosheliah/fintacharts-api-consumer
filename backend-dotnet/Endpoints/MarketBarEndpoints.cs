using System.Net;
using BackendDotnet.Data;
using BackendDotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendDotnet.Endpoints;

public class MarketBarEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/asset")
            .WithTags("Symbol Market Bars");
        group
            .MapGet("/providers", GetProviders)
            .WithSummary("Get a provider list");
        group
            .MapGet("/instruments", GetInstruments)
            .WithSummary("Get a filtered and paginated instrument list");
        group
            .MapGet("/history/{symbol}", GetHistory)
            .WithSummary("Get the aggregated bar data for the specified symbol");
        group
            .MapGet("/test/token", TestGetToken)
            .WithSummary("Test token state");
        group
            .Map("/ws/{symbol}", ConnectQuoteWebSocket)
            .WithSummary("Connect to a web socket which yields market asset quote status updates in real time");
    }

    public static IResult GetProviders(AssetService assetService)
    {
        return Results.Ok(assetService.GetProviderList());
    }

    public static async Task<IResult> GetInstruments(
        [FromQuery] string? symbol,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromQuery] string? provider,
        AssetService assetService
    )
    {
        var instruments = await assetService.GetInstruments(symbol, page, size, provider);
        return Results.Ok(instruments);
    }

    public static async Task<IResult> GetHistory(
        [FromRoute] string symbol,
        AssetService assetService,
        HttpRequest httpRequest
    )
    {
        var timeNow = DateTime.Now.Date;
        var now = DateOnly.FromDateTime(timeNow);
        var aMonthBack = DateOnly.FromDateTime(timeNow.Subtract(TimeSpan.FromDays(30)));
        var history = await assetService.GetHistory(symbol, "simulation", 1, "day", aMonthBack, now);
        if (history == null)
            return Results.NotFound();
        return Results.Ok(history);
    }

    public static async Task ConnectQuoteWebSocket(
        string symbol,
        HttpContext context,
        AssetService assetService,
        QuoteWebSocketRelayService relayService
    )
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        var instrument = assetService.GetInstrument(symbol);
        if (instrument == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        await relayService.HandleClientWebSocketAsync(socket, instrument);
    }

    public static async Task<IResult> TestGetToken(AuthTokenService tokenService)
    {
        var token = await tokenService.GetAccessTokenAsync();
        return Results.Ok(new { token, timestamp = DateTime.UtcNow });
    }
}