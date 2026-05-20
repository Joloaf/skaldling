namespace Skaldling.Api.Domain;

// Minimal hero entity to start out with
// The hero avatar parts added using JSONB at a later stage.
// We will expand upon this later, e.g. adding relationship to adventures/journeys and achievements.
public class Hero
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AchievementPoints { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}