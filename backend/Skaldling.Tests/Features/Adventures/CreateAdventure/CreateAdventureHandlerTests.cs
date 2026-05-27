using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Skaldling.Api.Domain;
using Skaldling.Api.Features.Adventures.CreateAdventure;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Adventures.CreateAdventure;

public class CreateAdventureHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;
    private readonly FakeTimeProvider _time;

    public CreateAdventureHandlerTests()
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
    public async Task HandleAsync_creates_adventure_blueprint_when_hero_and_theme_exist()
    {
        // Happy path - Full adventure draft (1 Adventure + 5 Days + 5 Branches + N Nodes + N AdventureTasks)
        var hero = await CreateTestHero();
        var theme = await _db.Themes.FirstAsync();
        var handler = new CreateAdventureHandler(_db, _time);
        var command = BuildValidCommand(hero.Id, theme.Id);

        var (outcome, response) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateAdventureHandler.Outcome.AdventureCreated);
        response.Should().NotBeNull();
        response!.HeroId.Should().Be(hero.Id);
        response.Title.Should().Be(command.Title);

        var adventure = await _db.Adventures
            .Include(a => a.Days).ThenInclude(d => d.Branches).ThenInclude(b => b.Nodes)
            .Include(a => a.AdventureTasks)
            .FirstOrDefaultAsync(a => a.Id == response.Id);

        adventure.Should().NotBeNull();
        adventure!.Status.Should().Be(AdventureStatus.Draft);
        adventure.Tone.Should().Be(AdventureTone.Cozy);
        adventure.NarrativeStyle.Should().Be(NarrativeStyle.TaskIntegrated);
        adventure.Days.Should().HaveCount(5);
        adventure.AdventureTasks.Should().HaveCount(5);
        adventure.Days.SelectMany(d => d.Branches).Should().HaveCount(5);
        adventure.Days.SelectMany(d => d.Branches).SelectMany(b => b.Nodes).Should().HaveCount(5);

        // Each day has exactly one branch with one node referencing one task - This is default for the MVP.
        foreach (var day in adventure.Days)
        {
            day.Branches.Should().HaveCount(1);
            day.Branches.Single().Nodes.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task HandleAsync_returns_HeroNotFound_when_hero_is_missing()
    {
        var theme = await _db.Themes.FirstAsync();
        var handler = new CreateAdventureHandler(_db, _time);
        var command = BuildValidCommand(Guid.NewGuid(), theme.Id);

        var (outcome, response) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateAdventureHandler.Outcome.HeroNotFound);
        response.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_ThemeNotFound_when_theme_is_missing()
    {
        var hero = await CreateTestHero();
        var handler = new CreateAdventureHandler(_db, _time);
        var command = BuildValidCommand(hero.Id, Guid.NewGuid());

        var (outcome, response) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateAdventureHandler.Outcome.ThemeNotFound);
        response.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_HeroAlreadyOnAdventure_when_hero_has_an_active_adventure()
    {
        var hero = await CreateTestHero();
        var theme = await _db.Themes.FirstAsync();
        var handler = new CreateAdventureHandler(_db, _time);

        var firstCommand = BuildValidCommand(hero.Id, theme.Id);
        var (firstOutcome, _) = await handler.HandleAsync(firstCommand, CancellationToken.None);
        firstOutcome.Should().Be(CreateAdventureHandler.Outcome.AdventureCreated);

        var secondCommand = BuildValidCommand(hero.Id, theme.Id);
        var (secondOutcome, secondResponse) = await handler.HandleAsync(secondCommand, CancellationToken.None);

        secondOutcome.Should().Be(CreateAdventureHandler.Outcome.HeroAlreadyOnAdventure);
        secondResponse.Should().BeNull();
    }

    private async Task<Hero> CreateTestHero()
    {
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = "Test Hero",
            ReadingAge = 7,
            AchievementPoints = 0,
            CreatedAt = _time.GetUtcNow(),
            UpdatedAt = _time.GetUtcNow(),
            // Not including any hero avatar sprites as they are not needed for the adventure draft.
        };
        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync();
        return hero;
    }

    private static CreateAdventureCommand BuildValidCommand(Guid heroId, Guid themeId)
    {
        return new CreateAdventureCommand(
            HeroId: heroId,
            ThemeId: themeId,
            Title: "Test Adventure",
            Tone: AdventureTone.Cozy,
            NarrativeStyle: NarrativeStyle.TaskIntegrated,
            Moral: null,
            FinaleReward: null,
            Days: Enumerable.Range(1, 5).Select(dayNumber => new DayCommand(
                DayNumber: dayNumber,
                Tasks: new[]
                {
                    new TaskCommand("Task description", TaskDifficulty.Medium, 20)
                }
            )).ToArray()
        );
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}