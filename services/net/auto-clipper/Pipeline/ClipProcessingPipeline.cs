using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Audio;
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;
using TNO.Services.AutoClipper.LLM;

namespace TNO.Services.AutoClipper.Pipeline;

public class ClipProcessingPipeline
{
    private readonly IAudioNormalizer _audioNormalizer;
    private readonly IAzureSpeechTranscriptionService _speechTranscriber;
    private readonly IAzureVideoIndexerClient? _videoIndexerClient;
    private readonly IClipSegmentationService _clipSegmentation;
    private readonly AutoClipperOptions _options;
    private readonly ILogger<ClipProcessingPipeline> _logger;

    public ClipProcessingPipeline(
        IAudioNormalizer audioNormalizer,
        IAzureSpeechTranscriptionService speechTranscriber,
        IClipSegmentationService clipSegmentation,
        IOptions<AutoClipperOptions> options,
        ILogger<ClipProcessingPipeline> logger,
        IAzureVideoIndexerClient? videoIndexerClient = null)
    {
        _audioNormalizer = audioNormalizer;
        _speechTranscriber = speechTranscriber;
        _videoIndexerClient = videoIndexerClient;
        _clipSegmentation = clipSegmentation;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ClipProcessingResult> ExecuteAsync(ClipProcessingContext context, CancellationToken cancellationToken)
    {
        var language = !string.IsNullOrWhiteSpace(context.Request.Language)
            ? context.Request.Language!
            : !string.IsNullOrWhiteSpace(context.StationProfile.Transcription.Language)
                ? context.StationProfile.Transcription.Language
                : _options.DefaultTranscriptLanguage;

        var provider = context.StationProfile.Transcription.Provider ?? "azure_speech";
        IReadOnlyList<TimestampedTranscript> segments;
        string workingPath;

        if (string.Equals(provider, "azure_video_indexer", StringComparison.OrdinalIgnoreCase))
        {
            // Use Azure Video Indexer - upload original file directly (no normalization needed)
            if (_videoIndexerClient == null)
                throw new InvalidOperationException("Video Indexer client is not configured but provider is set to azure_video_indexer");

            workingPath = context.SourcePath;
            var personModelId = ResolvePersonModelId(context.StationProfile.Transcription);

            _logger.LogInformation("Using Video Indexer provider (PersonModel: {PersonModel})", personModelId ?? "none");

            var viRequest = new VideoIndexerRequest
            {
                Language = language,
                PersonModelId = personModelId,
                IncludeSpeakerLabels = context.StationProfile.Transcription.IncludeSpeakerLabels
            };

            segments = await _videoIndexerClient.TranscribeAsync(context.SourcePath, viRequest, cancellationToken);
        }
        else
        {
            // Use Azure Speech (default) - requires audio normalization
            _logger.LogInformation("Using Azure Speech provider");

            workingPath = await _audioNormalizer.NormalizeAsync(context.SourcePath, context.TargetSampleRate, cancellationToken);
            var transcriptionRequest = new SpeechTranscriptionRequest
            {
                Language = language,
                EnableSpeakerDiarization = context.StationProfile.Transcription.Diarization,
                SpeakerCount = context.StationProfile.Transcription.MaxSpeakers,
                DiarizationMode = context.StationProfile.Transcription.DiarizationMode
            };

            segments = await _speechTranscriber.TranscribeAsync(workingPath, transcriptionRequest, cancellationToken);
        }

        var segmentationSettings = BuildSegmentationSettings(context.StationProfile);
        var clipDefinitions = await _clipSegmentation.GenerateClipsAsync(segments, segmentationSettings, cancellationToken);

        return new ClipProcessingResult(workingPath, language, segments, clipDefinitions, segmentationSettings);
    }

    /// <summary>
    /// Resolves the Person Model ID from station profile configuration.
    /// </summary>
    private static string? ResolvePersonModelId(StationTranscriptionProfile transcription)
    {
        if (string.IsNullOrWhiteSpace(transcription.PersonModelKey))
            return null;

        if (transcription.PersonModels.TryGetValue(transcription.PersonModelKey, out var modelId))
            return modelId;

        return null;
    }

    private static ClipSegmentationSettings BuildSegmentationSettings(StationProfile profile)
    {
        return new ClipSegmentationSettings
        {
            PromptOverride = string.IsNullOrWhiteSpace(profile.Text.LlmPrompt) ? null : profile.Text.LlmPrompt,
            ModelOverride = string.IsNullOrWhiteSpace(profile.Text.LlmModel) ? null : profile.Text.LlmModel,
            TemperatureOverride = profile.Text.LlmTemperature,
            SystemPrompt = string.IsNullOrWhiteSpace(profile.Text.SystemPrompt) ? null : profile.Text.SystemPrompt,
            PromptCharacterLimit = profile.Text.PromptCharacterLimit,
            MaxStories = profile.Text.MaxStories,
            KeywordPatterns = profile.Heuristics.KeywordPatterns?.ToArray(),

            HeuristicPatternEntries = profile.Heuristics.PatternEntries?
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Pattern))
                .Select(p => new HeuristicPatternSetting
                {
                    Pattern = p.Pattern!,
                    Weight = p.Weight,
                    Category = string.IsNullOrWhiteSpace(p.Category) ? null : p.Category,
                    Note = p.Note
                })
                .ToArray(),
            HeuristicBoundaryWeight = profile.Text.HeuristicBoundaryWeight,
            KeywordCategories = profile.Text.KeywordCategories?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}









