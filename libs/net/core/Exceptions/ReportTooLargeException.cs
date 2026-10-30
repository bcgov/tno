namespace TNO.Core.Exceptions;

/// <summary>
/// A report exceeds the supported size and must be reduced before it can run.
/// </summary>
public class ReportTooLargeException : BadRequestException
{
    /// <summary>
    /// The actionable message shown when previewing or sending an oversized report.
    /// </summary>
    public const string UserMessage = "This report has grown too large to run. Remove stories or narrow the date range and filters, then refresh the report before previewing or sending it again. Existing stories have not been removed.";

    /// <summary>
    /// Creates a report size validation failure.
    /// </summary>
    public ReportTooLargeException() : base(UserMessage) { }
}
