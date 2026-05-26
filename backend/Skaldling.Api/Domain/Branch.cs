namespace Skaldling.Api.Domain;

public class Branch
{
    public Guid Id { get; set; }
    public Guid DayId { get; set; }
    public Day Day { get; set; } = default!;
    public string? BranchLabel { get; set; }
    public int Order { get; set; }
    public List<Node> Nodes { get; set; } = new();
}