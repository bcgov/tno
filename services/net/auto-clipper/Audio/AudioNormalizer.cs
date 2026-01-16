using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TNO.Services.AutoClipper.Audio;

/// <summary>
/// AudioNormalizer class, provides a way to normalize files to ensure we only send wav files for transcription.
/// </summary>
public class AudioNormalizer : IAudioNormalizer
{
    private readonly ILogger<AudioNormalizer> _logger;

    public AudioNormalizer(ILogger<AudioNormalizer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Create a new wav file if required.
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="targetSampleRate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> NormalizeAsync(string sourceFile, int targetSampleRate, CancellationToken? cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(sourceFile) ?? ".";
        var normalizedFile = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(sourceFile)}.{targetSampleRate}hz.normalized.wav");

        if (File.Exists(normalizedFile))
        {
            var sourceInfo = new FileInfo(sourceFile);
            var normalizedInfo = new FileInfo(normalizedFile);
            if (normalizedInfo.LastWriteTimeUtc >= sourceInfo.LastWriteTimeUtc)
            {
                _logger.LogDebug("Using existing normalized file {File}", normalizedFile);
                return normalizedFile;
            }
        }

        _logger.LogInformation("Normalizing audio {Source} -> {Dest}", sourceFile, normalizedFile);
        Directory.CreateDirectory(directory);

        var process = new Process();
        if (OperatingSystem.IsWindows())
        {
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/c ffmpeg -y -i \"{sourceFile}\" -ar {targetSampleRate} -ac 1 -c:a pcm_s16le \"{normalizedFile}\"";
        }
        else
        {
            process.StartInfo.FileName = "/bin/sh";
            process.StartInfo.Arguments = $"-c \"ffmpeg -y -i '{sourceFile}' -ar {targetSampleRate} -ac 1 -c:a pcm_s16le '{normalizedFile}' 2>&1\"";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken ?? default);
        if (process.ExitCode != 0)
        {
            _logger.LogError("ffmpeg normalization failed: {Output}", output);
            throw new InvalidOperationException($"Failed to normalize audio: {sourceFile}");
        }
        return normalizedFile;
    }
}
