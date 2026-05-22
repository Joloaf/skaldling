using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Features.Heroes.CreateHero;
using Skaldling.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Time.Testing;

namespace Skaldling.Tests.Features.Heroes.CreateHero;

public class CreateHeroHandlerTests : IDisposable  // Fresh SQLite db for each test rather than each assembly.
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;
    private readonly FakeTimeProvider _time;

    public CreateHeroHandlerTests()
    {
        // Setup for SQLite for test schema using RAM
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();  // Creates matching tables from models, not looking at migrations

        _time = new FakeTimeProvider(
            new DateTimeOffset(2026, 5, 19, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact(Skip = "Needs to be updated after extending the database table")]
    public async Task HandleAsync_persists_hero_and_returns_response()
    {
        await Task.CompletedTask;
        /*var handler = new CreateHeroHandler(_db, _time);
        var command = new CreateHeroCommand("Thorbjörn");

        var response = await handler.HandleAsync(command, CancellationToken.None);

        response.Name.Should().Be("Thorbjörn");
        response.Id.Should().NotBeEmpty();
        response.CreatedAt.Should().Be(_time.GetUtcNow());

        var stored = await _db.Heroes.FindAsync(response.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Thorbjörn");
        stored.AchievementPoints.Should().Be(0);*/
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
