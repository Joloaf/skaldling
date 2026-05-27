namespace Skaldling.Api.Infrastructure.Configuration;

public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-5-20250929";
    public int MaxRetries { get; set; } = 3;
    public int MaxTokens { get; set; } = 8000;
    public bool EnablePromptCaching { get; set; } = true;
}
