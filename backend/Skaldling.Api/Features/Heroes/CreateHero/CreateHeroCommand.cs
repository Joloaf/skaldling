namespace Skaldling.Api.Features.Heroes.CreateHero;

public record CreateHeroCommand(string Name, int ReadingAge, Guid[] AvatarSpriteIds);