using Skaldling.Api.Domain;

namespace Skaldling.Api.Features.Sprites.ListSprites;

public record ListSpritesQuery(SpriteType? Type, ArchetypeFamily? ArchetypeFamily);