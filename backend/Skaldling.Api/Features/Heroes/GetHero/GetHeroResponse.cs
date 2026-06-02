namespace Skaldling.Api.Features.Heroes.GetHero;

public record GetHeroResponse(
    Guid Id,
    string Name,
    int ReadingAge,
    int AchievementPoints,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid[] AvatarSpriteIds,
    Guid? ActiveAdventureId,
    string? ActiveAdventureTitle,
    PastAdventureSummary[] PastAdventures);

    public record PastAdventureSummary(
        Guid Id,
        string Title,
        DateTimeOffset CompletedAt);