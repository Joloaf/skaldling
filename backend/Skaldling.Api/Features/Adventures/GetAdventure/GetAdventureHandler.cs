using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Adventures.GetAdventure;

public class GetAdventureHandler
{
    private readonly SkaldlingDbContext _db;

    public GetAdventureHandler(SkaldlingDbContext db) => _db = db;

    public async Task<GetAdventureResponse?> HandleAsync(Guid adventureId, CancellationToken cancellationToken)
    {
        // Worth looking into a better way of pulling the data from the database once the project grows. This is fine for the MVP.
        var adventure = await _db.Adventures
            .AsNoTracking()
            .Include(a => a.Hero)
            .Include(a => a.Theme)
            .Include(a => a.Days)
                .ThenInclude(d => d.Branches)
                    .ThenInclude(b => b.Nodes)
                        .ThenInclude(n => n.AdventureTask)
            .FirstOrDefaultAsync(a => a.Id == adventureId, cancellationToken);

        if (adventure is null) return null;

        return new GetAdventureResponse(
            adventure.Id,
            adventure.HeroId,
            adventure.Hero.Name,
            adventure.Hero.AchievementPoints,
            adventure.Title,
            adventure.Theme.Name,
            adventure.Tone,
            adventure.NarrativeStyle,
            adventure.Moral,
            adventure.FinaleReward,
            adventure.Status,
            adventure.CreatedAt,
            adventure.UpdatedAt,
            adventure.Days
                .OrderBy(d => d.DayNumber)
                .Select(MapDay)
                .ToArray());
    }

    private static PlayDayDto MapDay(Day day) => new(
        day.Id,
        day.DayNumber,
        day.NarrativeIntro,
        day.NarrativeConvergence,
        day.Branches
            .OrderBy(b => b.Order)
            .SelectMany(b => b.Nodes)
            .OrderBy(n => n.Order)
            .Select(MapNode)
            .ToArray());

    private static PlayNodeDto MapNode(Node node) => new(
        node.Id,
        node.Order,
        node.NarrativeText,
        node.SceneType,
        MapAdventureTask(node.AdventureTask));

    private static PlayAdventureTaskDto MapAdventureTask(AdventureTask task) => new(
        task.Id,
        task.Description,
        task.Difficulty,
        task.PointValue,
        task.IsCompleted);
}