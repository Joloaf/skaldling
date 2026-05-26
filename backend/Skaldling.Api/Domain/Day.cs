namespace Skaldling.Api.Domain;

public class Day
{
    public Guid Id { get; set; }
    public Guid AdventureId { get; set; }
    public Adventure Adventure { get; set; } = default!;
    public int DayNumber { get; set; }
    public string? NarrativeIntro { get; set; }
    public string? NarrativeConvergence { get; set; }
    public List<Branch> Branches { get; set; } = new();
}