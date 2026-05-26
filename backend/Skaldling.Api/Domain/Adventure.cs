namespace Skaldling.Api.Domain;

public class Adventure
{
    public Guid Id { get; set; }
    public Guid HeroId { get; set; }
    public Hero Hero { get; set; } = default!;
    public Guid ThemeId { get; set; }
    public Theme Theme { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public AdventureTone Tone { get; set; }
    public NarrativeStyle NarrativeStyle { get; set; } = NarrativeStyle.TaskIntegrated;
    public string? Moral { get; set; }
    public string? FinaleReward { get; set; }
    public AdventureStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Day> Days { get; set; } = new();
    public List<AdventureTask> AdventureTasks { get; set; } = new();
}

public enum AdventureTone { Cozy, Epic, Mysterious, Comedic, Spooky }

public enum AdventureStatus { Draft, Generating, Ready, Active, Completed, Abandoned }

public enum NarrativeStyle { TaskIntegrated, TaskIndependent }