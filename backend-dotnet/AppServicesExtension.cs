using BackendDotnet.Data;
using BackendDotnet.Services;
using Microsoft.AspNetCore.Http.Json;
using Serilog;
using System.Text.Json;

namespace BackendDotnet;

public static class AppServicesExtension
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors();
        builder.AddDocumentation();
        builder.AddSettings();
        builder.AddJsonOptions();
        builder.AddSerilog();
        builder.InjectDataServices();
    }

    private static void AddDocumentation(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen(options =>
        //{
        //    options.CustomSchemaIds(
        //        type =>
        //            type.FullName?.Replace('+', '.')
        //        );
        //    options.InferSecuritySchemes();
        //});
    }

    private static void AddSettings(this WebApplicationBuilder builder)
    {
        DotNetEnv.Env.Load();
        var BASE_URI = Environment.GetEnvironmentVariable("BASE_URI");
        var AUTH_REALM = Environment.GetEnvironmentVariable("AUTH_REALM");
        var AUTH_USERNAME = Environment.GetEnvironmentVariable("AUTH_USERNAME");
        var AUTH_PASSWORD = Environment.GetEnvironmentVariable("AUTH_PASSWORD");
        var WSS_URI = Environment.GetEnvironmentVariable("WSS_URI");
        var QUESTDB_CONNECTION_STRING = Environment.GetEnvironmentVariable("QUESTDB_CONNECTION_STRING");
        if (
            string.IsNullOrEmpty(BASE_URI)
            || string.IsNullOrEmpty(AUTH_REALM)
            || string.IsNullOrEmpty(AUTH_USERNAME)
            || string.IsNullOrEmpty(AUTH_PASSWORD)
            || string.IsNullOrEmpty(WSS_URI)
            || string.IsNullOrEmpty(QUESTDB_CONNECTION_STRING)
        )
        {
            throw new InvalidOperationException($"Missing required environment variables");
        }
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"Base:Uri", BASE_URI},
            {"Auth:Realm", AUTH_REALM},
            {"Auth:Username", AUTH_USERNAME},
            {"Auth:Password", AUTH_PASSWORD},
            {"Wss:Uri", WSS_URI},
            {"QuestDB:ConnectionString", QUESTDB_CONNECTION_STRING},
        });
    }

    private static void AddJsonOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });
    }

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }

    private static void InjectDataServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR(); // WebSocket connections
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<AuthTokenService>();
        builder.Services.AddSingleton<AssetCache>();
        builder.Services.AddSingleton<SymbolBarCache>();
        builder.Services.AddSingleton<QuestDBClient>();
        builder.Services.AddTransient<MarketService>();
    }
}