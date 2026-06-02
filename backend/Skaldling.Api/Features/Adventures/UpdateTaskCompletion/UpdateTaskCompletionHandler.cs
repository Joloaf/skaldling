using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Adventures.UpdateTaskCompletion;

public class UpdateTaskCompletionHandler
{
    private readonly SkaldlingDbContext _db;
    private readonly TimeProvider _time;

    public UpdateTaskCompletionHandler(SkaldlingDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public enum Outcome
    {
        TaskUpdated,
        AdventureNotFound,
        TaskNotInAdventure,
        AdventureNotActive
    }

    public async Task<(Outcome outcome, UpdateTaskCompletionResponse? response)> HandleAsync(
        Guid adventureId,
        Guid taskId,
        UpdateTaskCompletionCommand command,
        CancellationToken cancellationToken)
    {
        var adventure = await _db.Adventures
            .Include(a => a.AdventureTasks)
            .Include(a => a.Days)
            .ThenInclude(d => d.Branches)
            .ThenInclude(b => b.Nodes)
            .FirstOrDefaultAsync(a => a.Id == adventureId, cancellationToken);

        if (adventure is null)
        {
            return (Outcome.AdventureNotFound, null);
        }

        if (adventure.Status != AdventureStatus.Active)
        {
            return (Outcome.AdventureNotActive, null);
        }

        var task = adventure.AdventureTasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            return (Outcome.TaskNotInAdventure, null);
        }

        if (task.IsCompleted == command.IsCompleted)
        {
            var currentScore = adventure.AdventureTasks
                .Where(t => t.IsCompleted)
                .Sum(t => t.PointValue);

            return (Outcome.TaskUpdated,
                new UpdateTaskCompletionResponse(
                    task.Id, task.IsCompleted, currentScore, adventure.UpdatedAt));
        }

        task.IsCompleted = command.IsCompleted;

        var node = adventure.Days
            .SelectMany(d => d.Branches)
            .SelectMany(b => b.Nodes)
            .FirstOrDefault(n => n.AdventureTaskId == taskId);

        if (node is not null)
        {
            node.Status = command.IsCompleted ? NodeStatus.Completed : NodeStatus.Pending;
        }

        var adventureScore = adventure.AdventureTasks
            .Where(t => t.IsCompleted)
            .Sum(t => t.PointValue);

        adventure.UpdatedAt = _time.GetUtcNow();
        await _db.SaveChangesAsync(cancellationToken);

        return (Outcome.TaskUpdated,
            new UpdateTaskCompletionResponse(
                task.Id, task.IsCompleted, adventureScore, adventure.UpdatedAt));
    }
}