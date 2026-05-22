namespace Skaldling.Api.Features.Heroes.GetHero;

public static class GetHeroEndpoint
{
    public static IEndpointRouteBuilder MapGetHero(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/heroes/{id:guid}", async (
            Guid id,
            GetHeroHandler handler,
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