namespace Skaldling.Api.Features.Adventures.UpdateTaskCompletion;

public record UpdateTaskCompletionResponse(
    Guid TaskId,
    bool IsCompleted,
    int AdventureScore,
    DateTimeOffset UpdatedAt);