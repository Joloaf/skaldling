using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Adventures.StartAdventure;

public record StartAdventureResponse(
    Guid AdventureId,
    AdventureStatus Status,
    DateTimeOffset UpdatedAt);