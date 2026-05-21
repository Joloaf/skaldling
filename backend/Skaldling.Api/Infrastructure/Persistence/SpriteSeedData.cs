using Skaldling.Api.Domain;

namespace Skaldling.Api.Infrastructure.Persistence;

internal static class SpriteSeedData
{
    public static readonly Sprite[] All =
    [
        // --- Body archetypes (Layer 10) ---
        new Sprite
        {
            Id = new Guid("11111111-0001-0001-0000-000000000001"),
            Name = "human-base",
            Type = SpriteType.BodyArchetype,
            Description = "A slender human figure standing at ease, neutral posture.",
            AssetPath = "/sprites/body/human-base.svg",
            Layer = 10,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0001-0002-0000-000000000001"),
            Name = "beast-base",
            Type = SpriteType.BodyArchetype,
            Description = "A sturdy creaturely figure with broad shoulders and a slight crouch, standing on its hind legs.",
            AssetPath = "/sprites/body/beast-base.svg",
            Layer = 10,
            ArchetypeFamily = ArchetypeFamily.Beast,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0001-0003-0000-000000000001"),
            Name = "robot-base",
            Type = SpriteType.BodyArchetype,
            Description = "An angular humanoid robot frame in muted brushed steel, standing upright.",
            AssetPath = "/sprites/body/robot-base.svg",
            Layer = 10,
            ArchetypeFamily = ArchetypeFamily.Robot,
            IsDefault = true,
        },

        // --- Outfit bottoms (Layer 20) ---
        new Sprite
        {
            Id = new Guid("11111111-0006-0000-0000-000000000001"),
            Name = "brown-trousers",
            Type = SpriteType.OutfitBottom,
            Description = "Practical brown trousers tucked into short boots.",
            AssetPath = "/sprites/outfit-bottom/brown-trousers.svg",
            Layer = 20,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },

        // --- Outfit tops (Layer 30) ---
        new Sprite
        {
            Id = new Guid("11111111-0005-0000-0000-000000000001"),
            Name = "leather-vest",
            Type = SpriteType.OutfitTop,
            Description = "A worn leather vest with brass buttons over a coarse linen shirt.",
            AssetPath = "/sprites/outfit-top/leather-vest.svg",
            Layer = 30,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0005-0000-0000-000000000002"),
            Name = "woolen-tunic",
            Type = SpriteType.OutfitTop,
            Description = "A heavy moss-green woolen tunic, cinched at the waist.",
            AssetPath = "/sprites/outfit-top/woolen-tunic.svg",
            Layer = 30,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },

        // --- Faces (Layer 40) ---
        new Sprite
        {
            Id = new Guid("11111111-0002-0001-0000-000000000001"),
            Name = "human-face",
            Type = SpriteType.Face,
            Description = "A calm, attentive human face with a small mouth and a steady gaze.",
            AssetPath = "/sprites/face/human-face.svg",
            Layer = 40,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0002-0002-0000-000000000001"),
            Name = "beast-face",
            Type = SpriteType.Face,
            Description = "An expressive creature face with a warm-toned snout and watchful eyes.",
            AssetPath = "/sprites/face/beast-face.svg",
            Layer = 40,
            ArchetypeFamily = ArchetypeFamily.Beast,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0002-0003-0000-000000000001"),
            Name = "robot-face",
            Type = SpriteType.Face,
            Description = "A smooth metallic faceplate with sensor eyes and a thin speaker grille.",
            AssetPath = "/sprites/face/robot-face.svg",
            Layer = 40,
            ArchetypeFamily = ArchetypeFamily.Robot,
            IsDefault = true,
        },

        // --- Eyes (Layer 50) ---
        new Sprite
        {
            Id = new Guid("11111111-0003-0000-0000-000000000001"),
            Name = "green-eyes",
            Type = SpriteType.Eyes,
            Description = "Sharp green eyes that catch the light.",
            AssetPath = "/sprites/eyes/green-eyes.svg",
            Layer = 50,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0003-0000-0000-000000000002"),
            Name = "brown-eyes",
            Type = SpriteType.Eyes,
            Description = "Warm brown eyes, gentle and curious.",
            AssetPath = "/sprites/eyes/brown-eyes.svg",
            Layer = 50,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },

        // --- Hair (Layer 60) ---
        new Sprite
        {
            Id = new Guid("11111111-0004-0000-0000-000000000001"),
            Name = "curly-red-hair",
            Type = SpriteType.Hair,
            Description = "Long curly rust-red hair tied with a thin leather thong, falling past the shoulders.",
            AssetPath = "/sprites/hair/curly-red-hair.svg",
            Layer = 60,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0004-0000-0000-000000000002"),
            Name = "short-black-hair",
            Type = SpriteType.Hair,
            Description = "Short jet-black hair, slightly tousled.",
            AssetPath = "/sprites/hair/short-black-hair.svg",
            Layer = 60,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
        new Sprite
        {
            Id = new Guid("11111111-0004-0000-0000-000000000003"),
            Name = "long-blonde-hair",
            Type = SpriteType.Hair,
            Description = "Long flowing honey-blonde hair that reaches the upper back.",
            AssetPath = "/sprites/hair/long-blonde-hair.svg",
            Layer = 60,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },

        // --- Accessories (Layer 70) ---
        new Sprite
        {
            Id = new Guid("11111111-0007-0000-0000-000000000001"),
            Name = "feathered-cap",
            Type = SpriteType.Accessory,
            Description = "A dusty-blue cap with a single eagle feather angling skyward.",
            AssetPath = "/sprites/accessory/feathered-cap.svg",
            Layer = 70,
            ArchetypeFamily = ArchetypeFamily.Human,
            IsDefault = true,
        },
    ];
}