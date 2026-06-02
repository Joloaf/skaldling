using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Adventures.ConfirmFinaleReward;

public record ConfirmFinaleRewardResponse(
    Guid AdventureId,
    AdventureStatus Status,
    int HeroAchievementPoints,
    int EarnedScore,
    DateTimeOffset CompletedAt);