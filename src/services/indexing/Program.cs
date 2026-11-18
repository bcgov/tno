namespace TNO.Services.Indexing;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the IndexingService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the Indexing service console program.
        var program = new IndexingService(args);
        return program.RunAsync();
    }
}
