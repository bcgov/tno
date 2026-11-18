namespace TNO.Services.Scheduler;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the SchedulerService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the Scheduler service console program.
        var program = new SchedulerService(args);
        return program.RunAsync();
    }
}
