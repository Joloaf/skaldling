using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Skaldling.Api.Domain;
using Skaldling.Api.Features.Heroes.GetHero;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Heroes.GetHero;

public class GetHeroHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;
    private readonly FakeTimeProvider _time;

    public GetHeroHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection).Options;
        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();
        _time = new FakeTimeProvider(
            new DateTimeOffset(2026, 5, 26, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task HandleAsync_returns_hero_when_found()
    {
        // Now using the helper due to rule of three, found in the bottom of the code.
        var hero = await CreateTestHero();

        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(hero.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response!.Id.Should().Be(hero.Id);
        response.Name.Should().Be(hero.Name);
        response.ReadingAge.Should().Be(hero.ReadingAge);
        response.ActiveAdventureId.Should().BeNull();
        response.ActiveAdventureTitle.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_null_when_hero_not_found()
    {
        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_active_adventure_when_hero_has_active_adventure()
    {
        var hero = await CreateTestHero();
        var theme = await _db.Themes.FirstAsync();
        var adventure = new Adventure
        {
            Id = Guid.NewGuid(),
            HeroId = hero.Id,
            ThemeId = theme.Id,
            Title = "Freja's first adventure",
            Tone = AdventureTone.Epic,
            NarrativeStyle = NarrativeStyle.TaskIntegrated,
            Status = AdventureStatus.Draft,
            Moral = null,
            FinaleReward = null,
            CreatedAt = _time.GetUtcNow(),
            UpdatedAt = _time.GetUtcNow()
        };
        _db.Adventures.Add(adventure);
        await _db.SaveChangesAsync();

        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(hero.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response!.ActiveAdventureId.Should().Be(adventure.Id);
        response.ActiveAdventureTitle.Should().Be("Freja's first adventure");
    }

    [Theory]
    [InlineData(AdventureStatus.Completed)]
    [InlineData(AdventureStatus.Abandoned)]
    public async Task HandleAsync_returns_null_active_adventure_when_only_ended_adventures_exist(
        AdventureStatus endedStatus)
    {
        var hero = await CreateTestHero();
        var theme = await _db.Themes.FirstAsync();
        var adventure = new Adventure
        {
            Id = Guid.NewGuid(),
            HeroId = hero.Id,
            ThemeId = theme.Id,
            Title = "Previous test adventure",
            Tone = AdventureTone.Mysterious,
            NarrativeStyle = NarrativeStyle.TaskIndependent,
            Status = endedStatus,
            Moral = null,
            FinaleReward = null,
            CreatedAt = _time.GetUtcNow(),
            UpdatedAt = _time.GetUtcNow()
        };
        _db.Adventures.Add(adventure);
        await _db.SaveChangesAsync();

        var handler = new GetHeroHandler(_db);
        var response = await handler.HandleAsync(hero.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response!.ActiveAdventureId.Should().BeNull();
        response.ActiveAdventureTitle.Should().BeNull();
    }

    private async Task<Hero> CreateTestHero()
    {
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = "Freja",
            ReadingAge = 8,
            AchievementPoints = 0,
            CreatedAt = _time.GetUtcNow(),
            UpdatedAt = _time.GetUtcNow()
        };
        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync();
        return hero;
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}