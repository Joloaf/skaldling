namespace Skaldling.Api.Features.Themes.ListThemes;

public record ListThemesResponse(ThemeDto[] Themes);

public record ThemeDto(Guid Id, string Name, string Description, bool IsDefault);