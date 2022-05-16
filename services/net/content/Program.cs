namespace TNO.Services.Content;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ContentService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static int Main(string[] args)
    {
        // Run the Content service console program.
        var program = new ContentService(args);
        return program.Run();
    }
}
