namespace TNO.Services.FFmpeg;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the FFmpegService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the FFmpeg service console program.
        var program = new FFmpegService(args);
        return program.RunAsync();
    }
}
