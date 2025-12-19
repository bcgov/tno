using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Audio;
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;
using TNO.Services.AutoClipper.LLM;

// TEMP HARNESS: delete this file/project once manual AutoClipper validation is complete.

var input = args.FirstOrDefault();
if (string.IsNullOrWhiteSpace(input) || !File.Exists(input))
{
    Console.WriteLine("Usage: dotnet run --project tools/auto-clipper-harness -- <path-to-media> [language] [outputDir]");
    return;
}

// TEMP HARNESS helper: try to load a .env file automatically so this console app can run standalone.
// When removing this harness, remove the helper as well.
var envFile = Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_ENV_FILE")
             ?? Path.Combine(AppContext.BaseDirectory, ".env");
LoadEnvFile(envFile);
if (!File.Exists(envFile))
{
    // dotnet run from repo root -> fall back to the project-relative .env
    var fallback = Path.Combine(Directory.GetCurrentDirectory(), "tools", "auto-clipper-harness", ".env");
    LoadEnvFile(fallback);
}

var outputDir = args.Length > 2 ? args[2] : Path.Combine(Path.GetDirectoryName(Path.GetFullPath(input)) ?? ".", "auto-clipper-harness-output");
Directory.CreateDirectory(outputDir);

using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(o => o.TimestampFormat = "HH:mm:ss "));
var stationCode = Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_STATION") ?? "CKNW";
var stationConfigPath = Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_STATION_PATH")
    ?? Path.Combine(Directory.GetCurrentDirectory(), "services", "net", "auto-clipper", "Config", "Stations");
var stationOptions = Options.Create(new AutoClipperOptions { StationConfigPath = stationConfigPath });
var stationConfiguration = new StationConfigurationService(stationOptions, loggerFactory.CreateLogger<StationConfigurationService>());
var stationProfile = stationConfiguration.GetProfile(stationCode);

var language = args.Length > 1
    ? args[1]
    : Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_LANGUAGE")
        ?? (!string.IsNullOrWhiteSpace(stationProfile.Transcription.Language) ? stationProfile.Transcription.Language : "en-US");
var sampleRate = int.TryParse(Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_SAMPLE_RATE"), out var sr)
    ? sr
    : (stationProfile.Transcription.SampleRate > 0 ? stationProfile.Transcription.SampleRate : 16000);

var audioNormalizer = new AudioNormalizer(loggerFactory.CreateLogger<AudioNormalizer>());
var workingFile = await audioNormalizer.NormalizeAsync(input, sampleRate);

var options = Options.Create(new AutoClipperOptions
{
    AzureSpeechKey = RequireEnv("AUTOCLIP_HARNESS_SPEECH_KEY"),
    AzureSpeechRegion = RequireEnv("AUTOCLIP_HARNESS_SPEECH_REGION"),
    LlmApiUrl = RequireEnv("AUTOCLIP_HARNESS_LLM_URL"),
    LlmApiKey = RequireEnv("AUTOCLIP_HARNESS_LLM_KEY"),
    LlmDeployment = RequireEnv("AUTOCLIP_HARNESS_LLM_DEPLOYMENT"),
    LlmApiVersion = Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_LLM_VERSION") ?? "2024-07-18",
    LlmPrompt = Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_PROMPT")
        ?? (string.IsNullOrWhiteSpace(stationProfile.Text.LlmPrompt) ? string.Empty : stationProfile.Text.LlmPrompt),
    LlmMaxStories = int.TryParse(Environment.GetEnvironmentVariable("AUTOCLIP_HARNESS_MAX_STORIES"), out var maxStories) ? maxStories : 5,
    VolumePath = Path.GetDirectoryName(Path.GetFullPath(input)) ?? ".",
    DefaultTranscriptLanguage = stationProfile.Transcription.Language ?? "en-US"
});

var speechLogger = loggerFactory.CreateLogger<AzureSpeechTranscriptionService>();
var llmLogger = loggerFactory.CreateLogger<ClipSegmentationService>();
var speechService = new AzureSpeechTranscriptionService(options, speechLogger);
var llmService = new ClipSegmentationService(new HttpClient(), options, llmLogger);

var transcriptionRequest = new SpeechTranscriptionRequest
{
    Language = language,
    EnableSpeakerDiarization = stationProfile.Transcription.Diarization,
    SpeakerCount = stationProfile.Transcription.MaxSpeakers,
    DiarizationMode = stationProfile.Transcription.DiarizationMode
};

Console.WriteLine($"[HARNESS] Transcribing {workingFile} ...");
var segments = await speechService.TranscribeAsync(workingFile, transcriptionRequest, CancellationToken.None);
Console.WriteLine($"[HARNESS] Received {segments.Count} transcript segments");

var fullTranscriptBody = BuildTranscriptDocument(segments);
var fullTranscriptPath = Path.Combine(outputDir, "transcript_full.txt");
await File.WriteAllTextAsync(fullTranscriptPath, fullTranscriptBody ?? string.Empty);
Console.WriteLine($"[HARNESS] Full transcript -> {fullTranscriptPath}");

var segmentationSettings = BuildSegmentationSettings(stationProfile);
Console.WriteLine("[HARNESS] Asking LLM for clip definitions ...");
var promptDebugPath = Path.Combine(outputDir, "llm_prompt_debug.txt");
await File.WriteAllTextAsync(promptDebugPath, BuildPromptDebug(segmentationSettings, segments));
Console.WriteLine($"[HARNESS] Saved LLM prompt -> {promptDebugPath}");
var clipDefinitions = (await llmService.GenerateClipsAsync(segments, segmentationSettings, CancellationToken.None))
    .OrderBy(c => c.Start)
    .ToArray();
Console.WriteLine($"[HARNESS] LLM returned {clipDefinitions.Length} clip candidates");

var index = 1;
foreach (var definition in clipDefinitions)
{
    var normalized = NormalizeClipDefinition(definition, segments);
    if (normalized == null)
    {
        Console.WriteLine($"[HARNESS] Skip invalid clip {definition.Title}");
        continue;
    }

    var transcriptSlice = ExtractTranscriptRange(segments, normalized.Start, normalized.End);
    var transcriptBody = BuildTranscriptDocument(transcriptSlice);
    if (string.IsNullOrWhiteSpace(transcriptBody))
    {
        Console.WriteLine($"[HARNESS] Empty transcript for clip {definition.Title}");
        continue;
    }

    var clipPath = await CreateClipFileAsync(input, outputDir, normalized.Start, normalized.End, index);
    var transcriptPath = Path.Combine(outputDir, $"clip_{index:00}.txt");
    await File.WriteAllTextAsync(transcriptPath, transcriptBody);
    Console.WriteLine($"[HARNESS] Saved clip #{index} -> {clipPath}\n[HARNESS] Transcript -> {transcriptPath}");
    index++;
}

Console.WriteLine("[HARNESS] Complete.");

static string BuildPromptDebug(ClipSegmentationSettings settings, IReadOnlyList<TimestampedTranscript> segments)
{
    var builder = new StringBuilder();
    builder.AppendLine("Prompt Override:");
    builder.AppendLine(settings?.PromptOverride ?? "<none>");
    builder.AppendLine();
    builder.AppendLine("Transcript Preview:");
    builder.AppendLine(BuildNumberedTranscript(segments));
    return builder.ToString();
}

static string RequireEnv(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Environment variable '{key}' must be set for the AutoClipper harness.");
    return value;
}

static void LoadEnvFile(string path)
{
    if (!File.Exists(path)) return;
    foreach (var rawLine in File.ReadAllLines(path))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var separator = line.IndexOf('=');
        if (separator <= 0) continue;
        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        Environment.SetEnvironmentVariable(key, value);
    }
}

static ClipSegmentationSettings BuildSegmentationSettings(StationProfile profile)
{
    return new ClipSegmentationSettings
    {
        PromptOverride = string.IsNullOrWhiteSpace(profile.Text.LlmPrompt) ? null : profile.Text.LlmPrompt,
        ModelOverride = string.IsNullOrWhiteSpace(profile.Text.LlmModel) ? null : profile.Text.LlmModel,
        SystemPrompt = string.IsNullOrWhiteSpace(profile.Text.SystemPrompt) ? null : profile.Text.SystemPrompt,
        PromptCharacterLimit = profile.Text.PromptCharacterLimit,
        MaxStories = profile.Text.MaxStories
    };
}

static ClipDefinition? NormalizeClipDefinition(ClipDefinition definition, IReadOnlyList<TimestampedTranscript> segments)
{
    if (segments.Count == 0) return null;
    var maxEnd = segments[^1].End;
    var start = definition.Start < TimeSpan.Zero ? TimeSpan.Zero : definition.Start;
    var end = definition.End > maxEnd ? maxEnd : definition.End;
    if (end <= start) return null;

    var first = segments.FirstOrDefault(s => s.End > start);
    var last = segments.LastOrDefault(s => s.Start < end);
    if (first == null || last == null) return null;
    start = first.Start;
    end = last.End;
    return end <= start ? null : definition with { Start = start, End = end };
}

static IReadOnlyList<TimestampedTranscript> ExtractTranscriptRange(IReadOnlyList<TimestampedTranscript> segments, TimeSpan start, TimeSpan end)
    => segments.Where(s => s.End > start && s.Start < end).ToArray();

static string BuildNumberedTranscript(IReadOnlyList<TimestampedTranscript> segments)
{
    if (segments == null || segments.Count == 0) return string.Empty;
    var sb = new StringBuilder();
    for (var i = 0; i < segments.Count; i++)
    {
        var segment = segments[i];
        if (string.IsNullOrWhiteSpace(segment.Text)) continue;
        sb.AppendLine($"{i + 1}. {FormatTimestamp(segment.Start)} --> {FormatTimestamp(segment.End)} :: {segment.Text.Trim()}");
    }
    return sb.ToString();
}

static string BuildTranscriptDocument(IReadOnlyList<TimestampedTranscript> segments)
{
    if (segments == null || segments.Count == 0) return string.Empty;
    var sb = new StringBuilder();
    var idx = 1;
    foreach (var segment in segments)
    {
        if (string.IsNullOrWhiteSpace(segment.Text)) continue;
        sb.AppendLine(idx.ToString());
        sb.AppendLine($"{FormatTimestamp(segment.Start)} --> {FormatTimestamp(segment.End)}");
        sb.AppendLine(segment.Text.Trim());
        sb.AppendLine();
        idx++;
    }
    return sb.ToString().Trim();
}

static string FormatTimestamp(TimeSpan value) => string.Format("{0:00}:{1:00}:{2:00}.{3:000}", (int)value.TotalHours, value.Minutes, value.Seconds, value.Milliseconds);

static async Task<string> CreateClipFileAsync(string srcFile, string outputDir, TimeSpan start, TimeSpan end, int index)
{
    Directory.CreateDirectory(outputDir);
    var dest = Path.Combine(outputDir, $"clip_{index:00}{Path.GetExtension(srcFile)}");
    var durationSeconds = Math.Max(1, (end - start).TotalSeconds);
    var process = new System.Diagnostics.Process();
    if (IsWindows())
    {
        process.StartInfo.FileName = "cmd";
        process.StartInfo.Arguments = $"/c ffmpeg -y -ss {start.TotalSeconds:0.###} -i \"{srcFile}\" -t {durationSeconds:0.###} -c copy \"{dest}\"";
    }
    else
    {
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -y -ss {start.TotalSeconds:0.###} -i '{srcFile}' -t {durationSeconds:0.###} -c copy '{dest}' 2>&1\"";
    }
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.CreateNoWindow = true;
    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new InvalidOperationException($"ffmpeg failed: {output}");
    return dest;
}

static bool IsWindows() => OperatingSystem.IsWindows();





