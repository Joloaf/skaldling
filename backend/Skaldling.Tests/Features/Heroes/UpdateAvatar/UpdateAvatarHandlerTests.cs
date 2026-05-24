using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Skaldling.Api.Domain;
using Skaldling.Api.Features.Heroes.UpdateAvatar;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Tests.Features.Heroes.UpdateAvatar;

public class UpdateAvatarHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SkaldlingDbContext _db;
    private readonly FakeTimeProvider _time;

    public UpdateAvatarHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<SkaldlingDbContext>()
            .UseSqlite(_connection).Options;
        _db = new SkaldlingDbContext(options);
        _db.Database.EnsureCreated();
        _time = new FakeTimeProvider(
            new DateTimeOffset(2026, 5, 19, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task HandleAsync_returns_HeroNotFound_when_hero_is_missing()
    {
        var sprites = await GetCompleteAvatarSpriteIds();
        var handler = new UpdateAvatarHandler(_db, _time);
        var command = new UpdateAvatarCommand(sprites);

        var (outcome, _) = await handler.HandleAsync(
            Guid.NewGuid(), command, CancellationToken.None);

        outcome.Should().Be(UpdateAvatarHandler.Outcome.HeroNotFound);
    }

    [Fact]
    public async Task HandleAsync_updates_avatar_and_returns_Update()
    {
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedAt = _time.GetUtcNow(),
            UpdatedAt = _time.GetUtcNow(),
        };
        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync();

        var sprites = await GetCompleteAvatarSpriteIds();
        var handler = new UpdateAvatarHandler(_db, _time);
        var command = new UpdateAvatarCommand(sprites);

        var (outcome, response) = await handler.HandleAsync(hero.Id, command, CancellationToken.None);

        outcome.Should().Be(UpdateAvatarHandler.Outcome.Updated);
        response!.AvatarSpriteIds.Should().BeEquivalentTo(sprites);
        response.UpdatedAt.Should().Be(_time.GetUtcNow());
    }

    private async Task<Guid[]> GetCompleteAvatarSpriteIds()
    {
        var allTypes = Enum.GetValues<SpriteType>();
        var ids = new List<Guid>();
        foreach (var type in allTypes)
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