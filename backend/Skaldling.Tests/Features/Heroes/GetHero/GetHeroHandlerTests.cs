using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Features.Heroes.GetHero;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Heroes.GetHero;

public class GetHeroHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;

    public GetHeroHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection).Options;
        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task HandleAsync_returns_hero_when_found()
    {
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = "Freja",
            AchievementPoints = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync();

        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(hero.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response!.Id.Should().Be(hero.Id);
        response.Name.Should().Be("Freja");
    }

    [Fact]
    public async Task HandleAsync_returns_null_when_hero_not_found()
    {
        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        response.Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}