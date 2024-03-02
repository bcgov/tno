namespace TNO.Services.ExtractQuotes;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ExtractQuotesService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the ExtractQuotes service console program.
        var program = new ExtractQuotesService(args);
        return program.RunAsync();
    }
}
