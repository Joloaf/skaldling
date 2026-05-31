using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Persistence;

namespace Skaldling.Api.Features.Dev.PreviewStories;

public class PreviewStoriesHandler
{
    private readonly SkaldlingDbContext _db;

    public PreviewStoriesHandler(SkaldlingDbContext db) => _db = db;

    public async Task<string> RenderAsync(CancellationToken cancellationToken)
    {
        var adventures = await _db.Adventures
            .AsNoTracking()
            .Include(a => a.Hero)
            .Include(a => a.Theme)
            .Include(a => a.AdventureTasks)
            .Include(a => a.Days).ThenInclude(d => d.Branches).ThenInclude(b => b.Nodes)
            .Where(a => a.Status == AdventureStatus.Ready
                     || a.Status == AdventureStatus.Active
                     || a.Status == AdventureStatus.Completed)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync(cancellationToken);

        var html = new StringBuilder();
        html.Append("""
            <!DOCTYPE html>
            <html lang="en"><head><meta charset="utf-8"><title>Story preview</title>
            <style>
              body { font-family: Georgia, serif; max-width: 720px; margin: 2rem auto;
                     padding: 0 1rem; line-height: 1.6; color: #222; }
              h1 { border-bottom: 2px solid #222; padding-bottom: 0.5rem; }
              h2 { margin-top: 3rem; color: #5b3a29; }
              h3 { margin-top: 2rem; color: #5b3a29; }
              .meta { color: #666; font-size: 0.9em; margin: 0.25rem 0;
                      font-family: system-ui, sans-serif; }
              .day { margin: 2rem 0; padding-left: 1rem; border-left: 3px solid #d4a373; }
              .narrative { white-space: pre-wrap; }
              .intro, .convergence { font-style: italic; color: #444; }
              .node { margin: 1.5rem 0; }
              .task-hint { font-family: system-ui, sans-serif; font-size: 0.85em;
                           color: #888; margin-top: 0.25rem; }
              .empty { color: #999; font-style: italic; }
              hr { border: 0; border-top: 1px dashed #ccc; margin: 4rem 0; }
            </style></head><body>
            """);

        html.Append($"<h1>Story preview — {adventures.Count} adventure(s)</h1>");
        html.Append("<p class=\"meta\">Dev-only view. Newest first. " +
                    "Adventures with status Draft/Generating excluded.</p>");

        if (adventures.Count == 0)
        {
            html.Append("<p class=\"empty\">No adventures with narrative yet. " +
                        "Generate one via the new-adventure flow.</p>");
        }

        var first = true;
        foreach (var adventure in adventures)
        {
            if (!first) html.Append("<hr/>");
            first = false;

            html.Append($"<h2>{Encode(adventure.Title)}</h2>");
            html.Append($"""
                <p class="meta">{Encode(adventure.Hero.Name)} (reading age {adventure.Hero.ReadingAge})
                · {Encode(adventure.Theme.Name)} · {adventure.Tone} · {adventure.NarrativeStyle}
                · <strong>{adventure.Status}</strong>
                · updated {adventure.UpdatedAt:yyyy-MM-dd HH:mm}</p>
                """);

            if (!string.IsNullOrWhiteSpace(adventure.Moral))
                html.Append($"<p class=\"meta\"><em>Moral:</em> {Encode(adventure.Moral)}</p>");
            if (!string.IsNullOrWhiteSpace(adventure.FinaleReward))
                html.Append($"<p class=\"meta\"><em>Finale reward:</em> {Encode(adventure.FinaleReward)}</p>");

            foreach (var day in adventure.Days.OrderBy(d => d.DayNumber))
            {
                html.Append($"<div class=\"day\"><h3>Day {day.DayNumber}</h3>");

                html.Append(string.IsNullOrWhiteSpace(day.NarrativeIntro)
                    ? "<p class=\"intro empty\">(no intro)</p>"
                    : $"<p class=\"intro narrative\">{Encode(day.NarrativeIntro)}</p>");

                var nodes = day.Branches.SelectMany(b => b.Nodes).OrderBy(n => n.Order);
                foreach (var node in nodes)
                {
                    var task = adventure.AdventureTasks.FirstOrDefault(t => t.Id == node.AdventureTaskId);
                    html.Append("<div class=\"node\">");
                    html.Append(string.IsNullOrWhiteSpace(node.NarrativeText)
                        ? "<p class=\"empty\">(no narrative)</p>"
                        : $"<p class=\"narrative\">{Encode(node.NarrativeText)}</p>");
                    if (task is not null)
                    {
                        html.Append($"<p class=\"task-hint\">↳ task: {Encode(task.Description)} " +
                                    $"({task.Difficulty}, {task.PointValue} pts)</p>");
                    }
                    html.Append("</div>");
                }

                html.Append(string.IsNullOrWhiteSpace(day.NarrativeConvergence)
                    ? "<p class=\"convergence empty\">(no convergence)</p>"
                    : $"<p class=\"convergence narrative\">{Encode(day.NarrativeConvergence)}</p>");

                html.Append("</div>");
            }
        }

        html.Append("</body></html>");
        return html.ToString();
    }

    private static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);
}