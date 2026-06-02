using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Adventures.StartAdventure;

public class StartAdventureHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public StartAdventureHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome
    {
        AdventureStarted,
        AdventureNotFound,
        NotInReadyStatus
    }

    public async Task<(Outcome outcome, StartAdventureResponse? response)> HandleAsync(
        Guid adventureId,
        CancellationToken cancellationToken)
    {
        var adventure = await _db.Adventures
            .FirstOrDefaultAsync(a => a.Id == adventureId, cancellationToken);

        if (adventure is null)
        {
            return (Outcome.AdventureNotFound, null);
        }

        if (adventure.Status != AdventureStatus.Ready)
        {
            return (Outcome.NotInReadyStatus, null);
        }

        adventure.Status = AdventureStatus.Active;
        adventure.UpdatedAt = _time.GetUtcNow();
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.AdventureStarted,
            new StartAdventureResponse(adventure.Id, adventure.Status, adventure.UpdatedAt));
    }
}