namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public record UpdateAvatarResponse(Guid HeroId, Guid[] AvatarSpriteIds, DateTimeOffset UpdatedAt);
