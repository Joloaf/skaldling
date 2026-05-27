using Skaldling.Api.Domain;

namespace Skaldling.Api.Infrastructure.StoryGeneration;

public interface IStoryGenerator
{
    Task<StoryGenerationResult> GenerateAsync(
        StoryGenerationRequest request,
        CancellationToken cancellationToken);
}

public record StoryGenerationRequest(Adventure Adventure, string HeroContextBlock);

public abstract record StoryGenerationResult
{
    public record Success(StoryGraph Graph) : StoryGenerationResult;
    public record Failure(string Reason) : StoryGenerationResult;
}

public record StoryGraph(StoryDay[] Days);

public record StoryDay(int DayNumber, string NarrativeIntro, string NarrativeConvergence, StoryNode[] Nodes);

// Branches are not included as the MVP does not have forking paths. This will be implemented at a later stage.

public record StoryNode(Guid AdventureTaskId, int Order, string NarrativeText, SceneType SceneType);
