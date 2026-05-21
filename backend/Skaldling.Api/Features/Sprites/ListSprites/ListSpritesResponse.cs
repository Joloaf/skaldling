using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Sprites.ListSprites;

public record ListSpritesResponse(SpriteDto[] Sprites);

public record SpriteDto(
    Guid Id,
    string Name,
    string Type,
    string Description,
    string AssetPath,
    int Layer,
    string ArchetypeFamily,
    bool IsDefault);