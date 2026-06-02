namespace Skaldling.Api.Features.Adventures.StartAdventure;

public static class StartAdventureEndpoint
{
    public static IEndpointRouteBuilder MapStartAdventure(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/adventures/{id:guid}/start", async (
            Guid id,
            StartAdventureHandler handler,
            CancellationToken cancellationToken) =>
        {
            var (outcome, response) = await handler.HandleAsync(id, cancellationToken);
            return outcome switch
            {
                StartAdventureHandler.Outcome.AdventureStarted => Results.Ok(response),
                StartAdventureHandler.Outcome.AdventureNotFound => Results.NotFound(),
                StartAdventureHandler.Outcome.NotInReadyStatus =>
                    Results.Problem(
                        title: "Adventure can only be started while in Ready status.",
                        statusCode: 409),
                _ => Results.StatusCode(500)
            };
        });

        return routes;
    }
}