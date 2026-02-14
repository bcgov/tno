namespace MMI.Services.SmtpRetry;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the SmtpRetryService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the SmtpRetry service console program.
        var program = new SmtpRetryService(args);
        return program.RunAsync();
    }
}
