namespace Skaldling.Api.Features.Adventures.GetAdventure;

public static class GetAdventureEndpoint
{
    public static IEndpointRouteBuilder MapGetAdventure(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/adventures/{id:guid}", async (
            Guid id,
            GetAdventureHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(id, cancellationToken);
            return response is null
                ? Results.NotFound()
                : Results.Ok(response);
        });

        return routes;
    }
}