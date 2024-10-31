using TNO.Services.Managers;
using TNO.Services;

namespace TNO.Services.FileUpload;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Run the file upload service.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the file upload service console program.
        var program = new FileUploadService(args);
        return program.RunAsync();
    }
}