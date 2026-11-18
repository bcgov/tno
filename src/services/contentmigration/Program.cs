namespace TNO.Services.ContentMigration;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the ContentMigrationService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the content migration service console program.
        var program = new ContentMigrationService(args);
        return program.RunAsync();
    }
}
