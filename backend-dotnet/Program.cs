using Serilog;
using BackendDotnet;
using BackendDotnet.Data;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using BackendDotnet.Services;
using BackendDotnet.Helpers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
    
void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddCors();
    // add documentation
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(
            type =>
                type.FullName?.Replace('+', '.')
            );
        options.InferSecuritySchemes();
    });
    // add settings
    DotNetEnv.Env.Load();
    EnvironmentHelpers.ApplyEnvironmentVariablesToConfiguration(builder, [
        new() { name = "BASE_URL", configName = "Base:Url" },
        new() { name = "AUTH_REALM", configName = "Auth:Realm" },
        new() { name = "AUTH_USERNAME", configName = "Auth:Username" },
        new() { name = "AUTH_PASSWORD", configName = "Auth:Password" },
        new() { name = "WSS_URI", configName = "Wss:Uri" },
        new() { name = "QUESTDB_PROTOCOL", configName = "QuestDB:Protocol" },
        new() { name = "QUESTDB_URL", configName = "QuestDB:Url" },
        new() { name = "QUESTDB_USERNAME", configName = "QuestDB:USERNAME" },
        new() { name = "QUESTDB_PASSWORD", configName = "QuestDB:PASSWORD" },
        new() { name = "CORS_FRONTEND", configName = "Cors:Frontend" }
    ]);
    // add json options
    builder.Services.Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
    });
    // add logger
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });
    // inject data services
    builder.Services.AddSignalR(); // WebSocket connections
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<AuthTokenService>();
    builder.Services.AddSingleton<AssetCache>();
    builder.Services.AddSingleton<SymbolBarCache>();
    builder.Services.AddSingleton<QuestDBClient>();
    builder.Services.AddTransient<MarketService>();
}

async Task Configure(WebApplication app)
{
    // ensure database schema
    using var scope = app.Services.CreateScope();
    var dbClient = scope.ServiceProvider.GetRequiredService<QuestDBClient>();
    await dbClient.EnsureSchema();
    // interactive endpoints
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    // add cors
    var frontend = app.Configuration["Cors:Frontend"];
    ArgumentNullException.ThrowIfNull(frontend);
    app.UseCors(x => x
        .WithOrigins(frontend)
        .AllowCredentials()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
    //app.UseHttpsRedirection();
}

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);
    AddServices(builder);
    var app = builder.Build();
    await Configure(app);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (
    ex.GetType().Name is not "StopTheHostException"
    && ex.GetType().Name is not "HostAbortedException"
)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
