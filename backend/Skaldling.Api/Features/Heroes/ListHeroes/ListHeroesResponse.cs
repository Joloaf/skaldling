namespace Skaldling.Api.Features.Heroes.ListHeroes;

public record ListHeroesResponse(HeroSummaryDto[] Heroes);

public record HeroSummaryDto(
    Guid Id,
    string Name,
    int AchievementPoints,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid[] AvatarSpriteIds);