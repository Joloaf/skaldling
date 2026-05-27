namespace Skaldling.Api.Features.Heroes.UpdateHeroDetails;

public record UpdateHeroDetailsResponse(Guid HeroId, string Name, int ReadingAge, DateTimeOffset UpdatedAt);