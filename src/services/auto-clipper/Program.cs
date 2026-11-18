namespace TNO.Services.AutoClipper;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the AutoClipperService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the Transcription service console program.
        var program = new AutoClipperService(args);
        return program.RunAsync();
    }
}
