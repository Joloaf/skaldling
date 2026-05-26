using Skaldling.Api.Domain;

namespace Skaldling.Api.Infrastructure.Persistence;

internal static class ThemeSeedData
{
    public static readonly Theme[] All =
    [
        new Theme
        {
            Id = new Guid("22222222-0001-0000-0000-000000000001"),
            Name = "Norse Fantasy",
            Description = "Mist-cloaked fjords, ancient longships, and creatures from the old sagas. " +
                          "Heroes brave the cold north, sailing wild seas, walking whispering forests, and meeting trolls and skalds alike.",
            IsDefault = true,
        },
    ];
}