namespace Skaldling.Api.Domain;

public class AdventureTask
{
    public Guid Id { get; set; }
    public Guid AdventureId { get; set; }
    public Adventure Adventure { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public TaskDifficulty Difficulty { get; set; }
    public int PointValue { get; set; }
    public bool IsCompleted { get; set; }
}

public enum TaskDifficulty { Easy, Medium, Hard }