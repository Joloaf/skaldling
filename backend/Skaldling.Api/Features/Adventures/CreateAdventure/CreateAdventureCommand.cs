using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Adventures.CreateAdventure;

public record CreateAdventureCommand(
    Guid HeroId,
    Guid ThemeId,
    string Title,
    AdventureTone Tone,
    NarrativeStyle NarrativeStyle,
    string? Moral,
    string? FinaleReward,
    DayCommand[] Days);

public record DayCommand(int DayNumber, TaskCommand[] Tasks);

public record TaskCommand(string Description, TaskDifficulty Difficulty, int PointValue);