namespace Skaldling.Api.Features.Heroes.CreateHero;

public record CreateHeroCommand(string Name, Guid[] AvatarSpriteIds);