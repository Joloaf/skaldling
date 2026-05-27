using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Adventures.CreateAdventure;

public class CreateAdventureHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public CreateAdventureHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome { AdventureCreated, HeroNotFound, ThemeNotFound, HeroAlreadyOnAdventure }

    public async Task<(Outcome outcome, CreateAdventureResponse? response)> HandleAsync(
        CreateAdventureCommand command,
        CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == command.HeroId, cancellationToken);
        if (hero is null)
        {
            return (Outcome.HeroNotFound, null);
        }

        var theme = await _db.Themes.FirstOrDefaultAsync(t => t.Id == command.ThemeId, cancellationToken);
        if (theme is null)
        {
            return (Outcome.ThemeNotFound, null);
        }

        var hasActive = await _db.Adventures.AnyAsync(
            a => a.HeroId == command.HeroId
                && a.Status != AdventureStatus.Completed
                && a.Status != AdventureStatus.Abandoned,
            cancellationToken);
        if (hasActive)
        {
            return (Outcome.HeroAlreadyOnAdventure, null);
        }

        var now = _time.GetUtcNow();
        var adventureId = Guid.NewGuid();

        var adventure = new Adventure
        {
            Id = adventureId,
            HeroId = hero.Id,
            ThemeId = theme.Id,
            Title = command.Title,
            Tone = command.Tone,
            NarrativeStyle = command.NarrativeStyle,
            Moral = command.Moral,
            FinaleReward = command.FinaleReward,
            Status = AdventureStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var dayCmd in command.Days.OrderBy(d => d.DayNumber))
        {
            var day = new Day
            {
                Id = Guid.NewGuid(),
                AdventureId = adventureId,
                DayNumber = dayCmd.DayNumber,
            };

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                DayId = day.Id,
                BranchLabel = null,
                Order = 0,
            };

            for (var i = 0; i < dayCmd.Tasks.Length; i++)
            {
                var taskCmd = dayCmd.Tasks[i];
                var task = new AdventureTask
                {
                    Id = Guid.NewGuid(),
                    AdventureId = adventureId,
                    Description = taskCmd.Description,
                    Difficulty = taskCmd.Difficulty,
                    PointValue = taskCmd.PointValue,
                    IsCompleted = false,
                };
                adventure.AdventureTasks.Add(task);

                var node = new Node
                {
                    Id = Guid.NewGuid(),
                    BranchId = branch.Id,
                    Order = i,
                    AdventureTaskId = task.Id,
                    Status = NodeStatus.Pending,
                };
                branch.Nodes.Add(node);
            }

            day.Branches.Add(branch);
            adventure.Days.Add(day);
        }

        _db.Adventures.Add(adventure);
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.AdventureCreated,
            new CreateAdventureResponse(adventure.Id, hero.Id, adventure.Title, adventure.CreatedAt));
    }
}