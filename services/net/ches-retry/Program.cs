namespace TNO.Services.ChesRetry;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ChesRetryService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the ChesRetry service console program.
        var program = new ChesRetryService(args);
        return program.RunAsync();
    }
}
