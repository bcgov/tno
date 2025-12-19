namespace TNO.Services.AutoClipper.Config;

public class StationProfile
{
    public string Name { get; set; } = "default";
    public int SampleRate { get; set; } = 16000;
    public StationTranscriptionProfile Transcription { get; set; } = new();
    public StationTextProfile Text { get; set; } = new();
    public StationHeuristicProfile Heuristics { get; set; } = new StationHeuristicProfile();
}

public class StationTranscriptionProfile
{
    public string Provider { get; set; } = "azure_speech";
    public bool Diarization { get; set; }
    public int? MaxSpeakers { get; set; }
    public string? DiarizationMode { get; set; } = "online";
    public string Language { get; set; } = "en-US";
    public int SampleRate { get; set; } = 16000;
}

public class StationTextProfile
{
    public double ChunkSizeSeconds { get; set; } = 3.0;
    public double ChunkOverlapRatio { get; set; } = 0.5;
    public double HeuristicBoundaryWeight { get; set; } = 0.15;
    public Dictionary<string, string> KeywordCategories { get; set; } = new();
    public bool LlmSegmentation { get; set; } = true;
    public string LlmModel { get; set; } = string.Empty;
    public float? LlmTemperature { get; set; }
    public string LlmPrompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public bool? LlmDiarization { get; set; } = null;
    public int? PromptCharacterLimit { get; set; }
    public int? MaxStories { get; set; }
}

public class StationHeuristicProfile
{
    public Dictionary<string, double> BoundaryWeights { get; set; } = new();
    public List<string> KeywordPatterns { get; set; } = new();
    public List<StationHeuristicPattern> PatternEntries { get; set; } = new();
}

public class StationHeuristicPattern
{
    public string Pattern { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? Category { get; set; }
    public string? Note { get; set; }
}


