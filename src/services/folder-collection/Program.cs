namespace TNO.Services.FolderCollection;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the FolderCollectionService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the FolderCollection service console program.
        var program = new FolderCollectionService(args);
        return program.RunAsync();
    }
}
