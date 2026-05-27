using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Features.Themes.ListThemes;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Themes.ListThemes;

public class ListThemesHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;

    public ListThemesHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection).Options;
        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task HandleAsync_returns_seeded_themes()
    {
        var handler = new ListThemesHandler(_db);
        var query = new ListThemesQuery();

        var response = await handler.HandleAsync(query, CancellationToken.None);

        response.Themes.Should().NotBeEmpty();
        response.Themes.Should().Contain(t =>
            t.Name == "Norse Fantasy"
            && t.IsDefault
            && !string.IsNullOrWhiteSpace(t.Description));
    }

    [Fact]
    public async Task HandleAsync_orders_default_themes_first()
    {
        var handler = new ListThemesHandler(_db);
        var query = new ListThemesQuery();

        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Kind of redundant at MVP stage of project, will matter more once the project grows pass MVP.
        response.Themes.First().IsDefault.Should().BeTrue();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}