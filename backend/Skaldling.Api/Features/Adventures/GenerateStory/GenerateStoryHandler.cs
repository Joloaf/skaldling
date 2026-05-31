using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;
using Skaldling.Api.Infrastructure.StoryGeneration;

namespace Skaldling.Api.Features.Adventures.GenerateStory;

public class GenerateStoryHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly IStoryGenerator _storyGenerator;
    private readonly HeroContextAssembler _heroContext;
    private readonly TimeProvider _time;
    private readonly ILogger<GenerateStoryHandler> _logger;

    public GenerateStoryHandler(
        SkaldlingDbContext db,
        IStoryGenerator storyGenerator,
        HeroContextAssembler heroContext,
        TimeProvider time,
        ILogger<GenerateStoryHandler> logger)
    {
        _db = db;
        _storyGenerator = storyGenerator;
        _heroContext = heroContext;
        _time = time;
        _logger = logger;
    }

    public enum Outcome
    {
        StoryGenerated,
        AdventureNotFound,
        NotInDraftStatus,
        GenerationFailed
    }

    public async Task<(Outcome outcome, GenerateStoryResponse? response)> HandleAsync(
        Guid adventureId,
        CancellationToken cancellationToken)
    {
        var adventure = await _db.Adventures
            .Include(a => a.Hero)
            .Include(a => a.Theme)
            .Include(a => a.Days).ThenInclude(d => d.Branches).ThenInclude(b => b.Nodes)
            .Include(a => a.AdventureTasks)
            .FirstOrDefaultAsync(a => a.Id == adventureId, cancellationToken);

        if (adventure is null)
        {
            return (Outcome.AdventureNotFound, null);
        }

        if (adventure.Status != AdventureStatus.Draft)
        {
            return (Outcome.NotInDraftStatus, null);
        }

        adventure.Status = AdventureStatus.Generating;
        adventure.UpdatedAt = _time.GetUtcNow();
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var heroBlock = await _heroContext.ComposeAsync(adventure.Hero, cancellationToken);
            var request = new StoryGenerationRequest(adventure, heroBlock);
            var result = await _storyGenerator.GenerateAsync(request, cancellationToken);

            if (result is StoryGenerationResult.Failure failure)
            {
                _logger.LogWarning(
                    "Story generation failed for adventure {AdventureId}: {Reason}",
                    adventureId, failure.Reason);

                adventure.Status = AdventureStatus.Draft;
                adventure.UpdatedAt = _time.GetUtcNow();
                await _db.SaveChangesAsync(cancellationToken);

                return (Outcome.GenerationFailed, null);
            }

            var success = (StoryGenerationResult.Success)result;
            ApplyStoryGraph(adventure, success.Graph);

            adventure.Status = AdventureStatus.Ready;
            adventure.UpdatedAt = _time.GetUtcNow();
            await _db.SaveChangesAsync(cancellationToken);

            return (Outcome.StoryGenerated,
                new GenerateStoryResponse(adventure.Id, adventure.Status, adventure.UpdatedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception during story generation for adventure {AdventureId}",
                adventureId);

            adventure.Status = AdventureStatus.Draft;
            adventure.UpdatedAt = _time.GetUtcNow();
            await _db.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    private static void ApplyStoryGraph(Adventure adventure, StoryGraph graph)
    {
        foreach (var storyDay in graph.Days)
        {
            var day = adventure.Days.First(d => d.DayNumber == storyDay.DayNumber);
            day.NarrativeIntro = storyDay.NarrativeIntro;
            day.NarrativeConvergence = storyDay.NarrativeConvergence;

            var branch = day.Branches.Single();
            foreach (var storyNode in storyDay.Nodes)
            {
                var node = branch.Nodes.First(n => n.AdventureTaskId == storyNode.AdventureTaskId);
                node.NarrativeText = storyNode.NarrativeText;
                node.SceneType = storyNode.SceneType;
            }
        }
    }
}