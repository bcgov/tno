namespace TNO.Services.EventHandler;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the EventHandlerService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the EventHandler service console program.
        var program = new EventHandlerService(args);
        return program.RunAsync();
    }
}
