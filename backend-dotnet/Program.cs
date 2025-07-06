using Serilog;
using BackendDotnet;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServices();
    var app = builder.Build();
    await app.Configure();
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
