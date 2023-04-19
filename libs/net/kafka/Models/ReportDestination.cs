namespace TNO.Kafka.Models;

/// <summary>
/// ReportDestination enum, provides a way to control where to send a report.
/// </summary>
[Flags]
public enum ReportDestination
{
    /// <summary>
    /// No destination specified.
    /// </summary>
    None = 0,
    /// <summary>
    /// Broadcast message with SignalR
    /// </summary>
    SignalR = 1,
    /// <summary>
    /// The Report Service listens for these.
    /// </summary>
    ReportingService = 2,
}
