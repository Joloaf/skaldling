using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using Tool = Anthropic.SDK.Common.Tool;  // Using 'Tool' from the Common namespace, disregarding the one from Messaging.
using Microsoft.Extensions.Options;
using Skaldling.Api.Domain;
using Skaldling.Api.Infrastructure.Configuration;

namespace Skaldling.Api.Infrastructure.StoryGeneration;

public class LlmStoryGenerator : IStoryGenerator
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;
    private readonly ILogger<LlmStoryGenerator> _logger;
    private readonly string _systemPromptIntegrated;
    private readonly string _systemPromptIndependent;

    public LlmStoryGenerator(
        IOptions<AnthropicOptions> options,
        ILogger<LlmStoryGenerator> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new AnthropicClient(_options.ApiKey);

        _systemPromptIntegrated = LoadEmbeddedPrompt("story_v1_integrated.txt");
        _systemPromptIndependent = LoadEmbeddedPrompt("story_v1_independent.txt");
    }

    public async Task<StoryGenerationResult> GenerateAsync(
        StoryGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var systemPrompt = request.Adventure.NarrativeStyle == NarrativeStyle.TaskIntegrated
            ? _systemPromptIntegrated
            : _systemPromptIndependent;

        var userPrompt = BuildUserPrompt(request);
        var tool = BuildSubmitStoryGraphTool();

        for (var attempt = 1; attempt <= _options.MaxRetries; attempt++)
        {
            _logger.LogInformation(
                "Story generation attempt {Attempt}/{Max} for adventure {AdventureId}",
                attempt, _options.MaxRetries, request.Adventure.Id);

            try
            {
                var systemMessages = _options.EnablePromptCaching
                    ? new List<SystemMessage> { new(systemPrompt, new CacheControl { Type = CacheControlType.ephemeral }) }
                    : new List<SystemMessage> { new(systemPrompt) };

                var parameters = new MessageParameters
                {
                    Model = _options.Model,
                    MaxTokens = _options.MaxTokens,
                    System = systemMessages,
                    Messages = new List<Message>
                    {
                        new(RoleType.User, userPrompt)
                    },
                    Tools = new List<Tool> { tool },
                    ToolChoice = new ToolChoice { Type = ToolChoiceType.Tool, Name = "submit_story_graph" },
                    PromptCaching = _options.EnablePromptCaching
                        ? PromptCacheType.FineGrained
                        : PromptCacheType.None
                };

                var response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

                var toolCall = response.ToolCalls.FirstOrDefault();
                if (toolCall is null)
                {
                    _logger.LogWarning(
                        "Attempt {Attempt} produced no tool call; retrying",
                        attempt);
                    continue;
                }

                var argumentsJson = toolCall.Arguments?.ToJsonString();
                if (string.IsNullOrWhiteSpace(argumentsJson))
                {
                    _logger.LogWarning(
                        "Attempt {Attempt} produced an empty tool-call arguments payload; retrying",
                        attempt);
                    continue;
                }

                var graph = JsonSerializer.Deserialize<StoryGraph>(
                    argumentsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    });

                if (graph is null || !ValidateGraph(graph, request.Adventure))
                {
                    _logger.LogWarning(
                        "Attempt {Attempt} produced an invalid graph; retrying",
                        attempt);
                    continue;
                }

                return new StoryGenerationResult.Success(graph);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Attempt {Attempt} failed with exception; retrying",
                    attempt);
            }
        }

        return new StoryGenerationResult.Failure(
            $"Story generation failed after {_options.MaxRetries} attempts.");
    }

    private string BuildUserPrompt(StoryGenerationRequest request)
    {
        var adventure = request.Adventure;

        var taskList = string.Join("\n", adventure.Days
            .OrderBy(d => d.DayNumber)
            .SelectMany(d => d.Branches.Single().Nodes
                .OrderBy(n => n.Order)
                .Select(n => new
                {
                    Day = d.DayNumber,
                    Order = n.Order,
                    TaskId = n.AdventureTaskId,
                    Task = adventure.AdventureTasks.First(t => t.Id == n.AdventureTaskId)
                })
                .Select(x => $"Day {x.Day}, Node {x.Order}, AdventureTaskId {x.TaskId}: \"{x.Task.Description}\" (Difficulty: {x.Task.Difficulty})")));

        var moralLine = adventure.Moral is not null
            ? $"Moral (weave structurally): {adventure.Moral}"
            : "Moral: (none — let the story unfold without explicit moral framing)";

        var rewardLine = adventure.FinaleReward is not null
            ? $"Finale reward (reference in Day 5 finale): {adventure.FinaleReward}"
            : "Finale reward: (none — focus the finale on completion satisfaction)";

        return $$"""
            ## READER
            Reading age: {{adventure.Hero.ReadingAge}}
            Use this to target English vocab complexity and sentence structure per
            the CEFR-by-age table in the system prompt. The reader is NOT the same
            person as the story's protagonist — write the protagonist as the genre
            calls for; tune the reading level to the reader.

            ## HERO (the story's protagonist)
            {{request.HeroContextBlock}}

            ## ADVENTURE SETUP
            Title: {{adventure.Title}}
            Theme: {{adventure.Theme.Name}} — {{adventure.Theme.Description}}
            Tone: {{adventure.Tone}}
            {{moralLine}}
            {{rewardLine}}

            ## TASKS (use these AdventureTaskIds verbatim when binding nodes)
            {{taskList}}

            Now call the submit_story_graph tool with the complete 5-day adventure narrative.
            """;
    }

    private static Tool BuildSubmitStoryGraphTool()
    {
        var schema = new
        {
            type = "object",
            properties = new
            {
                days = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            dayNumber = new { type = "integer", minimum = 1, maximum = 5 },
                            narrativeIntro = new { type = "string" },
                            narrativeConvergence = new { type = "string" },
                            nodes = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        adventureTaskId = new { type = "string", format = "uuid" },
                                        order = new { type = "integer", minimum = 0 },
                                        narrativeText = new { type = "string" },
                                        sceneType = new
                                        {
                                            type = "string",
                                            @enum = new[] { "Quest", "Encounter", "Discovery", "Reflection", "Climax", "Resolution" }
                                        }
                                    },
                                    required = new[] { "adventureTaskId", "order", "narrativeText", "sceneType" }
                                }
                            }
                        },
                        required = new[] { "dayNumber", "narrativeIntro", "narrativeConvergence", "nodes" }
                    },
                    minItems = 5,
                    maxItems = 5
                }
            },
            required = new[] { "days" }
        };

        var schemaJson = JsonSerializer.Serialize(schema);
        var function = new Function(
            name: "submit_story_graph",
            description: "Submits the complete story graph for the adventure. Call this tool exactly once with the full 5-day narrative.",
            parameters: JsonNode.Parse(schemaJson));

        return new Tool(function);
    }

    private static bool ValidateGraph(StoryGraph graph, Adventure adventure)
    {
        if (graph.Days.Length != 5) return false;
        if (graph.Days.Select(d => d.DayNumber).Distinct().Count() != 5) return false;
        if (graph.Days.Any(d => d.DayNumber < 1 || d.DayNumber > 5)) return false;

        var validTaskIds = adventure.AdventureTasks.Select(t => t.Id).ToHashSet();

        foreach (var day in graph.Days)
        {
            var expectedNodeCount = adventure.Days
                .First(d => d.DayNumber == day.DayNumber)
                .Branches.Single().Nodes.Count;

            if (day.Nodes.Length != expectedNodeCount) return false;
            if (day.Nodes.Any(n => !validTaskIds.Contains(n.AdventureTaskId))) return false;
            if (string.IsNullOrWhiteSpace(day.NarrativeIntro)) return false;
            if (string.IsNullOrWhiteSpace(day.NarrativeConvergence)) return false;
            if (day.Nodes.Any(n => string.IsNullOrWhiteSpace(n.NarrativeText))) return false;
        }

        return true;
    }

    private static string LoadEmbeddedPrompt(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(filename, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"Embedded prompt resource '{filename}' not found. Verify <EmbeddedResource> in .csproj.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}