namespace Skaldling.Api.Domain;

public class Hero
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ReadingAge { get; set; }
    public int AchievementPoints { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public AvatarConfig AvatarConfig { get; set; } = new AvatarConfig([]);
}

public record AvatarConfig(Guid[] SpriteIds);