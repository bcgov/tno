namespace TNO.Services.ContentAnalysis;

/// <summary>
/// Program class, runs the Content-Analysis service.
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
        var program = new ContentAnalysisService(args);
        return program.RunAsync();
    }
}
