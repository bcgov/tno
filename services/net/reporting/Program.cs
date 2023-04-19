namespace TNO.Services.Reporting;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ReportingService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the Reporting service console program.
        var program = new ReportingService(args);
        return program.RunAsync();
    }
}
