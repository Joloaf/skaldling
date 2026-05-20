using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.CreateHero;

public class CreateHeroHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public CreateHeroHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<CreateHeroResponse> HandleAsync(
        CreateHeroCommand command,
        CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            AchievementPoints = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateHeroResponse(hero.Id, hero.Name, hero.CreatedAt);
    }
}