namespace Skafetin.Api.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public const string MockProvider = "Mock";

    public string Provider { get; set; } = MockProvider;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
