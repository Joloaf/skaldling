using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Adventures.ConfirmFinaleReward;

public class ConfirmFinaleRewardHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public ConfirmFinaleRewardHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome
    {
        FinaleConfirmed,
        AdventureNotFound,
        NotActive
    }

    public async Task<(Outcome outcome, ConfirmFinaleRewardResponse? response)> HandleAsync(
        Guid adventureId,
        CancellationToken cancellationToken)
    {
        var adventure = await _db.Adventures
            .Include(a => a.Hero)
            .Include(a => a.AdventureTasks)
            .FirstOrDefaultAsync(a => a.Id == adventureId, cancellationToken);

        if (adventure is null)
        {
            return (Outcome.AdventureNotFound, null);
        }

        if (adventure.Status != AdventureStatus.Active)
        {
            return (Outcome.NotActive, null);
        }

        var earnedScore = adventure.AdventureTasks
            .Where(t => t.IsCompleted)
            .Sum(t => t.PointValue);

        adventure.Hero.AchievementPoints += earnedScore;

        adventure.Status = AdventureStatus.Completed;
        var now = _time.GetUtcNow();
        adventure.UpdatedAt = now;
        adventure.Hero.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.FinaleConfirmed,
            new ConfirmFinaleRewardResponse(
                adventure.Id,
                adventure.Status,
                adventure.Hero.AchievementPoints,
                earnedScore,
                adventure.UpdatedAt));
    }
}