namespace Skaldling.Api.Features.Heroes.CreateHero;

public record CreateHeroResponse(
    Guid Id,
    string Name,
    int ReadingAge,
    DateTimeOffset CreatedAt,
    Guid[] AvatarSpriteIds);