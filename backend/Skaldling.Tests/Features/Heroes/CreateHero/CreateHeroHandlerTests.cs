using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
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

        var options = new DbContextOptionsBuilder<SkaldlingDbContext>().UseSqlite(_connection).Options;

        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();  // Creates matching tables from models, not looking at migrations
        _time = new FakeTimeProvider(
            new DateTimeOffset(2026, 5, 19, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task HandleAsync_create_hero_with_complete_avatar()
    {
        // Happy path - Hero with all required avatar sprites, one of each type
        var allDefaultSprites = await GetCompleteAvatarSpriteIds();
        var handler = new CreateHeroHandler(_db, _time);
        var command = new CreateHeroCommand("Thorbjörn", allDefaultSprites);
        var (outcome, response) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateHeroHandler.Outcome.HeroCreated);
        response.Should().NotBeNull();
        response!.Name.Should().Be("Thorbjörn");
        response.AvatarSpriteIds.Should().BeEquivalentTo(allDefaultSprites);

        var stored = await _db.Heroes.FindAsync(response.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Thorbjörn");
    }

    [Fact]
    public async Task HandleAsync_returns_UnknownSpriteIds_when_a_sprite_is_missing()
    {
        // Giving sprite 0 a new guid that won't match expected sprite ids
        var validSprites = await GetCompleteAvatarSpriteIds();
        validSprites[0] = Guid.NewGuid();
        var handler = new CreateHeroHandler(_db, _time);
        var command = new CreateHeroCommand("Test", validSprites);
        var (outcome, _) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateHeroHandler.Outcome.UnknownSpriteIds);
    }

    [Fact]
    public async Task HandleAsync_returns_DuplicateTypes_when_two_sprites_share_a_type()
    {
        // Duplicate hair types - Triggering DuplicateTypes before it checks MissingTypes
        var humanBase = await _db.Sprites.SingleAsync(s => s.Name == "human-base");
        var humanFace = await _db.Sprites.SingleAsync(s => s.Name == "human-face");
        var curlyRedHair = await _db.Sprites.SingleAsync(s => s.Name == "curly-red-hair");
        var shortBlackHair = await _db.Sprites.SingleAsync(s => s.Name == "short-black-hair");
        var brownEyes = await _db.Sprites.SingleAsync(s => s.Name == "brown-eyes");
        var leatherVest = await _db.Sprites.SingleAsync(s => s.Name == "leather-vest");
        var brownTrousers = await _db.Sprites.SingleAsync(s => s.Name == "brown-trousers");

        var ids = new[]
        {
            humanBase.Id, humanFace.Id, curlyRedHair.Id, shortBlackHair.Id, brownEyes.Id, leatherVest.Id,
            brownTrousers.Id
        };

        var handler = new CreateHeroHandler(_db, _time);
        var command = new CreateHeroCommand("Test", ids);
        var (outcome, _) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateHeroHandler.Outcome.DuplicateTypes);
    }

    [Fact]
    public async Task HandleAsync_returns_MissingTypes_when_a_type_is_missing()
    {
        // No DuplicateTypes - Triggering MissingTypes as the Accessory type is missing
        var humanBase = await _db.Sprites.SingleAsync(s => s.Name == "human-base");
        var humanFace = await _db.Sprites.SingleAsync(s => s.Name == "human-face");
        var curlyRedHair = await _db.Sprites.SingleAsync(s => s.Name == "curly-red-hair");
        var brownEyes = await _db.Sprites.SingleAsync(s => s.Name == "brown-eyes");
        var leatherVest = await _db.Sprites.SingleAsync(s => s.Name == "leather-vest");
        var brownTrousers = await _db.Sprites.SingleAsync(s => s.Name == "brown-trousers");

        var ids = new[]
        {
            humanBase.Id, humanFace.Id, curlyRedHair.Id, brownEyes.Id, leatherVest.Id, brownTrousers.Id
        };

        var handler = new CreateHeroHandler(_db, _time);
        var command = new CreateHeroCommand("Test", ids);
        var (outcome, _) = await handler.HandleAsync(command, CancellationToken.None);

        outcome.Should().Be(CreateHeroHandler.Outcome.MissingTypes);
    }

    private async Task<Guid[]> GetCompleteAvatarSpriteIds()
    {
        var AllTypes = Enum.GetValues<SpriteType>();
        var ids = new List<Guid>();
        foreach (var type in AllTypes)
        {
            var sprite = await _db.Sprites.FirstAsync(s => s.Type == type);
            ids.Add(sprite.Id);
        }
        return ids.ToArray();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
