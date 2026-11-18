namespace TNO.Core.Extensions;

/// <summary>
/// ExceptionExtensions static class, provides extention methods for exceptions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Describe a failure without dumping exception data or unbounded payloads. Include the
    /// root cause first so it survives truncation, followed by its stack and the outer context.
    /// </summary>
    /// <param name="ex">The failure to describe.</param>
    /// <returns>A diagnostic of at most 6,000 characters.</returns>
    public static string GetDiagnosticSummary(this Exception ex)
    {
        static string Cap(string? value, int length)
            => value == null ? "" : value.Length <= length ? value : value[..(length - 14)] + "...[truncated]";

        var root = ex.GetBaseException();
        var summary = $"{root.GetType().Name}: {Cap(root.Message, 1000)}\n{Cap(root.StackTrace, 2000)}";
        if (!ReferenceEquals(root, ex))
            summary += $"\nContext: {ex.GetType().Name}: {Cap(ex.Message, 1000)}\n{Cap(ex.StackTrace, 1800)}";
        return Cap(summary, 6000);
    }

    /// <summary>
    /// Get all inner error messages
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string GetAllMessages(this Exception ex)
    {
        return $"{ex.Message} {ex.InnerException?.GetAllMessages()}";
    }


    /// <summary>
    /// Get full exception type name
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string GetTypeName(this Exception ex)
    {
        return ex.GetType().Name;
    }

    /// <summary>
    /// Throw an ArgumentNullException if the value is null.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception type="ArgumentNullException">The argument value cannot be null.</exception>
    public static T ThrowIfNull<T>(this T value, string paramName) where T : class
    {
        return value ?? throw new ArgumentNullException(paramName);
    }
}
