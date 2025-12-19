using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TNO.Services.AutoClipper.Audio;

public class AudioNormalizer : IAudioNormalizer
{
    private readonly ILogger<AudioNormalizer> _logger;

    public AudioNormalizer(ILogger<AudioNormalizer> logger)
    {
        _logger = logger;
    }

    public async Task<string> NormalizeAsync(string sourceFile, int targetSampleRate, CancellationToken cancellationToken = default)
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
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            _logger.LogError("ffmpeg normalization failed: {Output}", output);
            throw new InvalidOperationException($"Failed to normalize audio: {sourceFile}");
        }
        return normalizedFile;
    }
}
