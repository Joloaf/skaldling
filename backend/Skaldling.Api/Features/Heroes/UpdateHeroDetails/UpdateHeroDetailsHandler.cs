using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.UpdateHeroDetails;

public class UpdateHeroDetailsHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public UpdateHeroDetailsHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome { Updated, HeroNotFound }

    public async Task<(Outcome outcome, UpdateHeroDetailsResponse? response)> HandleAsync(
        Guid heroId,
        UpdateHeroDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);
        if (hero is null)
        {
            return (Outcome.HeroNotFound, null);
        }

        hero.Name = command.Name;
        hero.ReadingAge = command.ReadingAge;
        hero.UpdatedAt = _time.GetUtcNow();
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.Updated,
            new UpdateHeroDetailsResponse(hero.Id, hero.Name, hero.ReadingAge, hero.UpdatedAt));
    }
}