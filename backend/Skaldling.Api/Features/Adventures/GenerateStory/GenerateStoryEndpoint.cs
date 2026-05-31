namespace Skaldling.Api.Features.Adventures.GenerateStory;

public static class GenerateStoryEndpoint
{
    public static IEndpointRouteBuilder MapGenerateStory(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/adventures/{id:guid}/generate", async (
            Guid id,
            GenerateStoryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var (outcome, response) = await handler.HandleAsync(id, cancellationToken);
            return outcome switch
            {
                GenerateStoryHandler.Outcome.StoryGenerated => Results.Ok(response),
                GenerateStoryHandler.Outcome.AdventureNotFound => Results.NotFound(),
                GenerateStoryHandler.Outcome.NotInDraftStatus =>
                    Results.Problem(
                        title: "Story can only be generated for adventures in Draft status.",
                        statusCode: 409),
                GenerateStoryHandler.Outcome.GenerationFailed =>
                    Results.Problem(
                        title: "Story generation failed after multiple attempts. Please try again.",
                        statusCode: 503),
                _ => Results.StatusCode(500)
            };
        });

        return routes;
    }
}