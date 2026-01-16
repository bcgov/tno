namespace TNO.Services.AutoClipper.LLM;

public class ClipSegmentationSettings
{
    public string? PromptOverride { get; set; }
    public string? ModelOverride { get; set; }
    public float? TemperatureOverride { get; set; }
    public string? SystemPrompt { get; set; }
    public int? PromptCharacterLimit { get; set; }
    public int? MaxStories { get; set; }
    public IReadOnlyList<string>? KeywordPatterns { get; set; }
    public double? HeuristicBoundaryWeight { get; set; }
    public IReadOnlyDictionary<string, string>? KeywordCategories { get; set; }
    public IReadOnlyList<HeuristicPatternSetting>? HeuristicPatternEntries { get; set; }
}

public class HeuristicPatternSetting
{
    public string Pattern { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? Category { get; set; }
    public string? Note { get; set; }
}

