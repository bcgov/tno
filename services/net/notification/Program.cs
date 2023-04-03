namespace TNO.Services.Notification;

/// <summary>
/// Program static class, runs program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Create an instance of the NotificationService and run it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task<int> Main(string[] args)
    {
        // Run the Notification service console program.
        var program = new NotificationService(args);
        return program.RunAsync();
    }
}
