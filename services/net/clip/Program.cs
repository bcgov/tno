namespace TNO.Services.Clip;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ClipService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the clip service console program.
        var program = new ClipService(args);
        return program.RunAsync();
    }
}
