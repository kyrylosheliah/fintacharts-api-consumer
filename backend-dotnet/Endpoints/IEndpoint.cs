namespace BackendDotnet.Endpoints;

public interface IEndpoint {
    static abstract void Map(IEndpointRouteBuilder app);
}