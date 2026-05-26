namespace Skaldling.Api.Domain;

public class Node
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;
    public int Order { get; set; }
    public string? NarrativeText { get; set; }
    public SceneType? SceneType { get; set; }
    public Guid AdventureTaskId { get; set; }
    public AdventureTask AdventureTask { get; set; } = default!;
    public NodeStatus Status { get; set; } = NodeStatus.Pending;
}

public enum SceneType
{
    Quest,
    Encounter,
    Discovery,
    Reflection,
    Climax,
    Resolution
}

public enum NodeStatus { Pending, Completed }