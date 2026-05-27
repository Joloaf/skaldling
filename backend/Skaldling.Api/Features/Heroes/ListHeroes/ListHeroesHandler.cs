using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.ListHeroes;

public class ListHeroesHandler
{
    private readonly SkaldlingDbContext _db;

    public ListHeroesHandler(SkaldlingDbContext db)
    {
        _db = db;
    }

    public async Task<ListHeroesResponse> HandleAsync(
        ListHeroesQuery query,
        CancellationToken cancellationToken)
    {
        var heroes = await _db.Heroes
            .AsNoTracking()
            .OrderByDescending(h => h.UpdatedAt)
            .ToArrayAsync(cancellationToken);

        var rows = heroes
            .Select(h => new HeroSummaryDto(
                h.Id,
                h.Name,
                h.ReadingAge,
                h.AchievementPoints,
                h.CreatedAt,
                h.UpdatedAt,
                h.AvatarConfig.SpriteIds))
            .ToArray();

        return new ListHeroesResponse(rows);
    }
}