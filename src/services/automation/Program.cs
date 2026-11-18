namespace TNO.Services.Automation;

/// <summary>
/// Program class, runs the automation service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Entrypoint.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        var program = new AutomationService(args);
        return program.RunAsync();
    }
}
