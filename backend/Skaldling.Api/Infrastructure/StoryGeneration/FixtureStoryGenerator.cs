using Skaldling.Api.Domain;

namespace Skaldling.Api.Infrastructure.StoryGeneration;

// Pre-written story generator used for tests and as a fallback if the Claude API is unavailable.
public class FixtureStoryGenerator : IStoryGenerator
{
    // Builds a complete story day by day for the adventure.
    public Task<StoryGenerationResult> GenerateAsync(StoryGenerationRequest request, CancellationToken cancellationToken)
    {
        var days = request.Adventure.Days
            .OrderBy(d => d.DayNumber)
            .Select(BuildDay)
            .ToArray();

        var graph = new StoryGraph(days);

        // Using Task.FromResult to match the async interface used for the LLM Story Generator.
        return Task.FromResult<StoryGenerationResult>(new StoryGenerationResult.Success(graph));
    }

    // Build each day of the story - using narrative segments.
    private static StoryDay BuildDay(Day day)
    {
        var nodes = day.Branches
            .Single()  // Single branch in the MVP, forked paths will hopefully be implemented in a future update.
            .Nodes
            .OrderBy(n => n.Order)
            .Select((node, index) => new StoryNode(
                AdventureTaskId: node.AdventureTaskId,
                Order: node.Order,
                NarrativeText: NarrativeForDay(day.DayNumber, index),
                SceneType: SceneTypeForDay(day.DayNumber, index)))
            .ToArray();

        return new StoryDay(
            DayNumber: day.DayNumber,
            NarrativeIntro: IntroForDay(day.DayNumber),
            NarrativeConvergence: ConvergenceForDay(day.DayNumber),
            Nodes: nodes);
    }

    // Day-opening narratives for a 5-day story arc.
    private static string IntroForDay(int dayNumber) => dayNumber switch
    {
        1 => "On the first morning, mist still clung to the fjord and the longships rocked gently at their moorings. The hero stretched, breathed in the cold sea air, and felt the call of adventure rising.",
        2 => "The second day broke clear and bright. Yesterday's path had led further than expected; today there were new lands to cross and new strangers to meet.",
        3 => "Midway through the journey, the hero paused at a high vantage point. The fjord lay far below; the great forest stretched ahead. Decisions made today would shape what came next.",
        4 => "The fourth day brought heavier skies and a quickening pace. Something important waited at the end of the road, and the hero felt it pulling them forward.",
        5 => "On the final morning, the path was familiar at last — but the hardest part still lay ahead. The hero gripped the worn handle of their pack, took a breath, and set out.",
        _ => "A new day in the adventure."
    };

    // Day-closing narratives for a 5-day story arc.
    private static string ConvergenceForDay(int dayNumber) => dayNumber switch
    {
        1 => "As dusk settled, the hero made camp beneath an ancient pine. Sleep came easily; tomorrow would bring whatever it brought.",
        2 => "Stars rose. The hero rested, knowing the road ahead would not be easy — but also that they would not face it alone.",
        3 => "Night fell with the hero tucked into a hollow between three great stones. The forest hummed its quiet song.",
        4 => "The hero settled into uneasy rest, the climax of the journey looming. Tomorrow, everything would change.",
        5 => "And so the adventure ended — not with the great victory the hero had imagined, but with something quieter and more lasting. Home was a different shape now; so was the hero.",
        _ => "The day ended."
    };

    // Node-specific narratives with dayNumber and nodeIndex keys. Generic to fit any task.
    private static string NarrativeForDay(int dayNumber, int nodeIndex)
    {
        return (dayNumber, nodeIndex) switch
        {
            (1, 0) => "The hero set out from the fjord village, the morning still cold around them. A small task awaited at the edge of the wood — small but important, the kind that sets the tone for everything else.",
            (1, 1) => "Further into the wood, the hero met an old traveler who asked for help. Help given freely returns in unexpected ways, the hero remembered from the sagas.",
            (1, 2) => "Before the day's end, the hero paused at a wayside cairn and added a stone, as travelers had done for a thousand years.",
            (2, 0) => "The road climbed. The hero walked steadily, knowing that steady steps cover more ground than rushed ones.",
            (2, 1) => "A river crossing offered the day's challenge. The water was cold, the stones slippery — but the hero crossed.",
            (2, 2) => "On the far side, a small kindness for a fellow traveler made the hour pass faster.",
            (3, 0) => "The hero searched the great forest for the markers the old map mentioned. Patience was rewarded — the markers lay there, beneath the moss.",
            (3, 1) => "A puzzle barred the way: not a riddle, but a problem to think through carefully.",
            (3, 2) => "By evening the puzzle was solved, and the path lay open again.",
            (4, 0) => "Heavy clouds rolled in. The hero pressed on, knowing the weather would pass and the road would still be there.",
            (4, 1) => "An unexpected helper appeared at the crossroads — a small bit of help, but enough to change the day's outcome.",
            (4, 2) => "Before nightfall, the hero stood at the edge of the final valley, looking down at what waited there.",
            (5, 0) => "The first task of the final day called for steady hands and a clear head. The hero gave both.",
            (5, 1) => "The second challenge came from within. The hero thought back over the journey, and chose what to do next.",
            (5, 2) => "And then it was done. The hero stood blinking in the changed light, the journey complete, the world a little different than it had been five days ago.",
            _ => "The hero attended to the task before them, and moved one step closer to the journey's end."
        };
    }

    // Scene-type tagging mirroring what the LLM would produce for the storytelling.
    private static SceneType SceneTypeForDay(int dayNumber, int nodeIndex)
    {
        return (dayNumber, nodeIndex) switch
        {
            (1, _) => SceneType.Quest,
            (2, _) => SceneType.Encounter,
            (3, _) => SceneType.Discovery,
            (4, 0) => SceneType.Reflection,
            (4, _) => SceneType.Climax,
            (5, 2) => SceneType.Resolution,
            (5, _) => SceneType.Climax,
            _ => SceneType.Quest
        };
    }
}