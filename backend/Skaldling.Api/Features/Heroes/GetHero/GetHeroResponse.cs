namespace Skaldling.Api.Features.Heroes.GetHero;

public record GetHeroResponse(
    Guid Id,
    string Name,
    int AchievementPoints,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid[] AvatarSpriteIds);