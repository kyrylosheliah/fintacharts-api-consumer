using BackendDotnet.Filters;
using BackendDotnet.Endpoints;
using BackendDotnet.Models;

namespace BackendDotnet;

public static class AppEndpointsExtension {
    public static void MapEndpoints(this WebApplication app) {
        var endpoints = app
            .MapGroup("/api/v1")
            .AddEndpointFilter<RequestLoggingFilter>()
            .WithOpenApi();

        endpoints.MapMarketBarEndpoints();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint {
        TEndpoint.Map(app);
        return app;
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null) {
        return app
            .MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    private static void MapMarketBarEndpoints(this IEndpointRouteBuilder app) {
        app
            .MapPublicGroup()
            .MapEndpoint<MarketBarEndpoint>();
    }
}