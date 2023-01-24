namespace TNO.Services.FileCopy;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the FileCopyService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the FileCopy service console program.
        var program = new FileCopyService(args);
        return program.RunAsync();
    }
}
