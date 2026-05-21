using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Sprites.ListSprites;

public static class ListSpritesEndpoint
{
    public static IEndpointRouteBuilder MapListSprites(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/sprites", async (
            SpriteType? type,
            ArchetypeFamily? archetypeFamily,
            ListSpritesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new ListSpritesQuery(type, archetypeFamily);
            var response = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(response);
        });

        return routes;
    }
}