using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Sprites.ListSprites;

public class ListSpritesHandler
{
    private readonly SkaldlingDbContext _db;

    public ListSpritesHandler(SkaldlingDbContext db)
    {
        _db = db;
    }

    public async Task<ListSpritesResponse> HandleAsync(
        ListSpritesQuery query,
        CancellationToken cancellationToken)
    {
        var q = _db.Sprites.AsQueryable();

        if (query.Type is { } type)
        {
            q = q.Where(s => s.Type == type);
        }

        if (query.ArchetypeFamily is { } family)
        {
            q = q.Where(s => s.ArchetypeFamily == family);
        }

        var rows = await q
            .OrderBy(s => s.Layer)
            .ThenBy(s => s.Name)
            .Select(s => new SpriteDto(
                s.Id,
                s.Name,
                s.Type.ToString(),
                s.Description,
                s.AssetPath,
                s.Layer,
                s.ArchetypeFamily.ToString(),
                s.IsDefault))
            .ToArrayAsync(cancellationToken);

        return new ListSpritesResponse(rows);
    }
}