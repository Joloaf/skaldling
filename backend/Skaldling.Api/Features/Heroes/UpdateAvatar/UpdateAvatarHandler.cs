using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public class UpdateAvatarHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public UpdateAvatarHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome { Update, HeroNotFound, UnknownSpriteIds, DuplicatedTypes, MissingTypes }

    public async Task<(Outcome outcome, UpdateAvatarResponse? response)> HandleAsync(
        Guid heroId,
        UpdateAvatarCommand command,
        CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);
        if (hero is null)
        {
            return (Outcome.HeroNotFound, null);
        }

        var sprites = await _db.Sprites
            .Where(s => command.SpriteIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
        if (sprites.Count != command.SpriteIds.Length)
        {
            return (Outcome.UnknownSpriteIds, null);
        }

        var typeGroups = sprites.GroupBy(s => s.Type).ToList();
        if (typeGroups.Any(g => g.Count() > 1))
        {
            return (Outcome.DuplicatedTypes, null);
        }

        var typesPresent = typeGroups.Select(g => g.Key).ToHashSet();
        var typesRequired = Enum.GetValues<SpriteType>().ToHashSet();
        if (!typesRequired.SetEquals(typesPresent))
        {
            return (Outcome.MissingTypes, null);
        }

        hero.AvatarConfig = new AvatarConfig(command.SpriteIds);
        hero.UpdatedAt = _time.GetUtcNow();
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.Update, new UpdateAvatarResponse(hero.Id, command.SpriteIds, hero.UpdatedAt));
    }
}