using Microsoft.EntityFrameworkCore;
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

    public enum Outcome { HeroCreated, UnknownSpriteIds, DuplicateTypes, MissingTypes }

    public async Task<(Outcome outcome, CreateHeroResponse? response)> HandleAsync(
        CreateHeroCommand command,
        CancellationToken cancellationToken)
    {
        var sprites = await _db.Sprites
            .Where(s => command.AvatarSpriteIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
        if (sprites.Count != command.AvatarSpriteIds.Length)
        {
            return (Outcome.UnknownSpriteIds, null);
        }

        var typeGroups = sprites.GroupBy(s => s.Type).ToList();
        if (typeGroups.Any(g => g.Count() > 1))
        {
            return (Outcome.DuplicateTypes, null);
        }

        var typesPresent = typeGroups.Select(g => g.Key).ToHashSet();
        var typesRequired = Enum.GetValues<SpriteType>().ToHashSet();
        if (!typesRequired.SetEquals(typesPresent))
        {
            return (Outcome.MissingTypes, null);
        }

        var now = _time.GetUtcNow();
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            AchievementPoints = 0,
            CreatedAt = now,
            UpdatedAt = now,
            AvatarConfig = new AvatarConfig(command.AvatarSpriteIds),
        };

        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.HeroCreated, new CreateHeroResponse(hero.Id, hero.Name, hero.CreatedAt, command.AvatarSpriteIds));
    }
}