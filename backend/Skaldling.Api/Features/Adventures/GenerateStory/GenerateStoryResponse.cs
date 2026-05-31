using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Adventures.GenerateStory;

public record GenerateStoryResponse(
    Guid AdventureId,
    AdventureStatus Status,
    DateTimeOffset UpdatedAt);