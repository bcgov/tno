namespace TNO.Services.Command;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the CommandService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the command service console program.
        var program = new CommandService(args);
        return program.RunAsync();
    }
}
