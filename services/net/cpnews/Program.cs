namespace TNO.Services.CPNews;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the CPNewsService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the cpnews service console program.
        var program = new CPNewsService(args);
        return program.RunAsync();
    }
}
