using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Themes.ListThemes;

public class ListThemesHandler
{
    private readonly SkaldlingDbContext _db;

    public ListThemesHandler(SkaldlingDbContext db) => _db = db;

    public async Task<ListThemesResponse> HandleAsyn(ListThemesQuery query, CancellationToken cancellationToken)
    {
        var rows = await _db.Themes
            .AsNoTracking()
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new ThemeDto(t.Id, t.Name, t.Description, t.IsDefault))
            .ToArrayAsync(cancellationToken);

        return new ListThemesResponse(rows);
    }
}