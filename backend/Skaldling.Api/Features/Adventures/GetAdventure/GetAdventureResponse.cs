using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Adventures.GetAdventure;

public record GetAdventureResponse(
    Guid Id,
    Guid HeroId,
    string HeroName,
    int HeroAchievementPoints,
    string Title,
    string ThemeName,
    AdventureTone Tone,
    NarrativeStyle NarrativeStyle,
    string? Moral,
    string? FinaleReward,
    AdventureStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    PlayDayDto[] Days);

public record PlayDayDto(
    Guid Id,
    int DayNumber,
    string? NarrativeIntro,
    string? NarrativeConvergence,
    PlayNodeDto[] Nodes);

public record PlayNodeDto(
    Guid Id,
    int Order,
    string? NarrativeText,
    SceneType? SceneType,
    PlayAdventureTaskDto AdventureTask);

public record PlayAdventureTaskDto(
    Guid Id,
    string Description,
    TaskDifficulty Difficulty,
    int PointValue,
    bool IsCompleted);