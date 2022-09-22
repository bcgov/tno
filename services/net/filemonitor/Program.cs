namespace TNO.Services.FileMonitor;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the SyndicationService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the syndication service console program.
        var program = new FileMonitorService(args);
        return program.RunAsync();
    }
}
