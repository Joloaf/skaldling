using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.GetHero;

public class GetHeroHandler
{
    private readonly SkaldlingDbContext _db;

    public GetHeroHandler(SkaldlingDbContext db) => _db = db;

    public async Task<GetHeroResponse?> HandleAsync(Guid heroId, CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);

        if (hero is null) return null;

        var activeAdventure = await _db.Adventures
            .AsNoTracking()
            .Where(a => a.HeroId == heroId
                && a.Status != AdventureStatus.Completed
                && a.Status == AdventureStatus.Abandoned)
            .Select(a => new { a.Id, a.Title })
            .FirstOrDefaultAsync(cancellationToken);

        return new GetHeroResponse(
            hero.Id,
            hero.Name,
            hero.ReadingAge,
            hero.AchievementPoints,
            hero.CreatedAt,
            hero.UpdatedAt,
            hero.AvatarConfig.SpriteIds,
            activeAdventure?.Id,
            activeAdventure?.Title);
    }
}