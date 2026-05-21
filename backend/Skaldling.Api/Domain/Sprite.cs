namespace Skaldling.Api.Domain;

public class Sprite
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SpriteType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AssetPath { get; set; } = string.Empty;
    public int Layer { get; set; }
    public ArchetypeFamily ArchetypeFamily { get; set; }
    public bool IsDefault { get; set; }
    public Dictionary<string, object>? Tags { get; set; }
}

public enum SpriteType
{
    Hair,
    Eyes,
    Face,
    BodyArchetype,
    OutfitTop,
    OutfitBottom,
    Accessory,
}

public enum ArchetypeFamily
{
    Human,
    Beast,
    Robot,
}