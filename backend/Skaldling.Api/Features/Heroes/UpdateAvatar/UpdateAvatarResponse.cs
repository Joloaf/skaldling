namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public record UpdateAvatarResponse(Guid HeroId, Guid[] SpriteIds, DateTimeOffset UpdatedAt);
