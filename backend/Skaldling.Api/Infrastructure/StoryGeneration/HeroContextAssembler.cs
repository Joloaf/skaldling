using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Infrastructure.StoryGeneration;

// Building the hero description that we want to include in our LLM story building.

public class HeroContextAssembler
{
    private readonly SkaldlingDbContext _db;

    public HeroContextAssembler(SkaldlingDbContext db) => _db = db;

    public async Task<string> ComposeAsync(Hero hero, CancellationToken cancellationToken)
    {
        var spriteIds = hero.AvatarConfig.SpriteIds;
        var sprites = await _db.Sprites
            .AsNoTracking()
            .Where(s => spriteIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        // Sort sprites in the order we want their description to be read.
        var displayOrder = new[]
        {
            SpriteType.BodyArchetype, SpriteType.Face, SpriteType.Eyes, SpriteType.Hair,
            SpriteType.OutfitTop, SpriteType.OutfitBottom, SpriteType.Accessory
        };

        var orderedDescriptions = displayOrder
            .Select(type => sprites.FirstOrDefault(s => s.Type == type)?.Description)
            .Where(desc => !string.IsNullOrWhiteSpace(desc))
            .ToList();

        var bodyDescription = string.Join("; ", orderedDescriptions);

        return $"The story's protagonist is named {hero.Name}. {hero.Name}'s appearance: {bodyDescription}.";
    }
}