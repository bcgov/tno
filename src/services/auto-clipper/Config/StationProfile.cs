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
    /// <summary>
    /// Transcription provider: "azure_speech" or "azure_video_indexer"
    /// </summary>
    public string Provider { get; set; } = "azure_speech";

    public string Language { get; set; } = "en-US";
    public int SampleRate { get; set; } = 16000;

    // Azure Speech specific settings
    public bool Diarization { get; set; }
    public int? MaxSpeakers { get; set; }
    public string? DiarizationMode { get; set; } = "online";

    // Azure Video Indexer specific settings
    /// <summary>
    /// Dictionary of Person Model names to IDs for speaker identification.
    /// Example: { "news": "model-id-1", "sports": "model-id-2" }
    /// </summary>
    public Dictionary<string, string> PersonModels { get; set; } = new();

    /// <summary>
    /// Key to select which Person Model to use from PersonModels dictionary.
    /// </summary>
    public string? PersonModelKey { get; set; }

    /// <summary>
    /// Whether to include speaker labels (speaker1:, speaker2:, or named) in transcript.
    /// </summary>
    public bool IncludeSpeakerLabels { get; set; }
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


