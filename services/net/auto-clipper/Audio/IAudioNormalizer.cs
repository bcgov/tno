namespace TNO.Services.AutoClipper.Audio;

public interface IAudioNormalizer
{
    Task<string> NormalizeAsync(string sourceFile, int targetSampleRate, CancellationToken cancellationToken = default);
}
