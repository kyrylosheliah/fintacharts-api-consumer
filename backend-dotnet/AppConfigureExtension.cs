using BackendDotnet.Data;

namespace BackendDotnet;

public static class AppConfigureExtension {
    public static async Task Configure(this WebApplication app) {
        await app.EnsureDatabaseSchema();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }
        app.AddCors();
        //app.UseHttpsRedirection();
    }

    private static async Task EnsureDatabaseSchema(this WebApplication app) {
        using var scope = app.Services.CreateScope();
        var dbClient = scope.ServiceProvider.GetRequiredService<QuestDBClient>();
        await dbClient.EnsureSchema();
    }

    private static void AddCors(this WebApplication app)
    {
        var frontend = app.Configuration["Cors:Frontend"];
        ArgumentNullException.ThrowIfNull(frontend);
        app.UseCors(x => x
            .WithOrigins(frontend)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
    }
}