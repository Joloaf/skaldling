namespace Skaldling.Api.Features.Themes.ListThemes;

public static class ListThemesEndpoint
{
    public static IEndpointRouteBuilder MapListThemes(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/themes", async (
            ListThemesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new ListThemesQuery();
            var response = await handler.HandleAsyn(query, cancellationToken);
            return Results.Ok(response);
        });

        return routes;
    }
}