namespace Skaldling.Api.Features.Adventures.UpdateTaskCompletion;

public static class UpdateTaskCompletionEndpoint
{
    public static IEndpointRouteBuilder MapUpdateTaskCompletion(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/api/adventures/{adventureId:guid}/tasks/{taskId:guid}/completion", async (
            Guid adventureId,
            Guid taskId,
            UpdateTaskCompletionCommand command,
            UpdateTaskCompletionHandler handler,
            CancellationToken cancellationToken) =>
        {
            var (outcome, response) = await handler.HandleAsync(adventureId, taskId, command, cancellationToken);
            return outcome switch
            {
                UpdateTaskCompletionHandler.Outcome.TaskUpdated => Results.Ok(response),
                UpdateTaskCompletionHandler.Outcome.AdventureNotFound => Results.NotFound(),
                UpdateTaskCompletionHandler.Outcome.TaskNotInAdventure => Results.NotFound(),
                UpdateTaskCompletionHandler.Outcome.AdventureNotActive =>
                    Results.Problem(
                        title: "Task completion can only be updated while the adventure is Active.",
                        statusCode: 409),
                _ => Results.StatusCode(500)
            };
        });

        return routes;
    }
}