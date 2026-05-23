namespace Skaldling.Api.Features.Heroes.ListHeroes;

public static class ListHeroesEndpoint
{
    public static IEndpointRouteBuilder MapListHeroes(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/heroes", async (
            ListHeroesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new ListHeroesQuery();
            var response = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(response);
        });

        return routes;
    }
}