namespace Skaldling.Api.Features.Dev.PreviewStories;

public static class PreviewStoriesEndpoint
{
    public static IEndpointRouteBuilder MapPreviewStories(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/dev/stories", async (
            PreviewStoriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var html = await handler.RenderAsync(cancellationToken);
            return Results.Content(html, "text/html; charset=utf-8");
        });

        return routes;
    }
}