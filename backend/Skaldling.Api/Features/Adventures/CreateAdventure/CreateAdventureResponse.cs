namespace Skaldling.Api.Features.Adventures.CreateAdventure;

public record CreateAdventureResponse(
    Guid Id,
    Guid HeroId,
    string Title,
    DateTimeOffset CreatedAt);