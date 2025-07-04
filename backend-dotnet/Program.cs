using System.Text.Json;
using BackendDotnet.Services;
using Microsoft.AspNetCore.Http.Json;
using BackendDotnet.Data;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
var AUTH_URI = Environment.GetEnvironmentVariable("AUTH_URI");
var AUTH_REALM = Environment.GetEnvironmentVariable("AUTH_REALM");
var AUTH_USERNAME = Environment.GetEnvironmentVariable("AUTH_USERNAME");
var AUTH_PASSWORD = Environment.GetEnvironmentVariable("AUTH_PASSWORD");
var WSS_URI = Environment.GetEnvironmentVariable("WSS_URI");
var QUESTDB_CONNECTION_STRING = Environment.GetEnvironmentVariable("QUESTDB_CONNECTION_STRING");
if (
    string.IsNullOrEmpty(AUTH_URI)
    || string.IsNullOrEmpty(AUTH_REALM)
    || string.IsNullOrEmpty(AUTH_USERNAME)
    || string.IsNullOrEmpty(AUTH_PASSWORD)
    || string.IsNullOrEmpty(WSS_URI)
    || string.IsNullOrEmpty(QUESTDB_CONNECTION_STRING)
) {
    throw new InvalidOperationException($"Missing required environment variables");
}
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    {"Auth:Uri", AUTH_URI},
    {"Auth:Realm", AUTH_REALM},
    {"Auth:Username", AUTH_USERNAME},
    {"Auth:Password", AUTH_PASSWORD},
    {"Wss:Uri", WSS_URI},
    {"QuestDB:ConnectionString", QUESTDB_CONNECTION_STRING},
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// WebSocket connections
builder.Services.AddSignalR();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<MarketQuoteCache>();
builder.Services.AddSingleton<QuestDBClient>();
//builder.Services.AddSingleton<FintachartsService>(client =>
//{
//    client.BaseAddress = newUri(builder.Configuration["Fintacharts:BaseUrl"];
//    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Fintacharts:Token"]}");
//});
//builder.Services.AddSingleton<MarketService>();
//builder.Services.AddHostedService<WebSocketBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

var api = app.MapGroup("/api/v1");

// api.MapGet("/realtime/{symbol}", (string symbol, RealtimeCache cache) =>
// {
//     if (cache.TryGet(symbol, out var quote))
//         return Results.Ok(quote);
//     return Results.NotFound();
// });

// api.MapGet("/history/{symbol}", async (string symbol, QuestDbClient db) =>
// {
//     var bars = await db.GetOHLCVAsync(symbol);
//     return Results.Ok(bars);
// });

api.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

api.MapGet("/token", async (TokenService tokenService) =>
{
    var token = await tokenService.GetAccessTokenAsync();
    return Results.Ok(new { token, timestamp = DateTime.UtcNow });
});

app.Run();
