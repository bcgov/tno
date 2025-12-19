namespace TNO.Services.AutoClipper.LLM;

public class ClipSegmentationSettings
{
    public string? PromptOverride { get; set; }
    public string? ModelOverride { get; set; }
    public string? SystemPrompt { get; set; }
    public int? PromptCharacterLimit { get; set; }
    public int? MaxStories { get; set; }
}
