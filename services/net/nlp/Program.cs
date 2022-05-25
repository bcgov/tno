namespace TNO.Services.NLP;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the NLPService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the NLP service console program.
        var program = new NLPService(args);
        return program.RunAsync();
    }
}
