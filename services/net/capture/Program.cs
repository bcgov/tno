namespace TNO.Services.Capture;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the CaptureService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the capture service console program.
        var program = new CaptureService(args);
        return program.RunAsync();
    }
}
