using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.GetHero;

public class GetHeroHandler
{
    private readonly SkaldlingDbContext _db;

    public GetHeroHandler(SkaldlingDbContext db)
    {
        _db = db;
    }

    public async Task<GetHeroResponse?> HandleAsync(Guid heroId, CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);

        return hero is null
            ? null
            : new GetHeroResponse(
                hero.Id,
                hero.Name,
                hero.AchievementPoints,
                hero.CreatedAt,
                hero.UpdatedAt,
                hero.AvatarConfig.SpriteIds);
    }
}