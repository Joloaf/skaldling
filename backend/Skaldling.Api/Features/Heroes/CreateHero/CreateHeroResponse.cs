namespace Skaldling.Api.Features.Heroes.CreateHero;

public record CreateHeroResponse(Guid Id, string Name, DateTimeOffset CreatedAt);