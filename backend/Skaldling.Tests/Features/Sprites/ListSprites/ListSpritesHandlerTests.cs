using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Features.Sprites.ListSprites;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Sprites.ListSprites;

public class ListSpritesHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;

    public ListSpritesHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task HandleAsync_returns_all_sprites_when_no_filter_applied()
    {
        var handler = new ListSpritesHandler(_db);
        var query = new ListSpritesQuery(Type: null, ArchetypeFamily: null);

        var response = await handler.HandleAsync(query, CancellationToken.None);

        response.Sprites.Should().NotBeEmpty();
        response.Sprites.Should().BeInAscendingOrder(s => s.Layer);
    }

    [Fact]
    public async Task HandleAsync_filters_by_type()
    {
        var handler = new ListSpritesHandler(_db);
        var query = new ListSpritesQuery(Type: SpriteType.Hair, ArchetypeFamily: null);

        var response = await handler.HandleAsync(query, CancellationToken.None);

        response.Sprites.Should().NotBeEmpty();
        response.Sprites.Should().OnlyContain(s => s.Type == "Hair");
    }

    [Fact]
    public async Task HandleAsync_filters_by_archetype_family()
    {
        var handler = new ListSpritesHandler(_db);
        var query = new ListSpritesQuery(Type: null, ArchetypeFamily: ArchetypeFamily.Human);

        var response = await handler.HandleAsync(query, CancellationToken.None);

        response.Sprites.Should().NotBeEmpty();
        response.Sprites.Should().OnlyContain(s => s.ArchetypeFamily == "Human");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
