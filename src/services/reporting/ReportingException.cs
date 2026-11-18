namespace TNO.Services.Reporting;

public class ReportingException : Exception
{
    #region Properties
    public int ReportId { get; set; }
    public string? ReportName { get; set; }
    public long? InstanceId { get; set; }
    public ReportingErrors Error { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportingException object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instanceId"></param>
    /// <param name="error"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ReportingException(
        int reportId,
        long? instanceId,
        ReportingErrors error,
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
        this.ReportId = reportId;
        this.InstanceId = instanceId;
        this.Error = error;
    }

    /// <summary>
    /// Creates a new instance of a ReportingException object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="reportName"></param>
    /// <param name="instanceId"></param>
    /// <param name="error"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ReportingException(
        int reportId,
        string? reportName,
        long? instanceId,
        ReportingErrors error,
        string? message,
        Exception? innerException)
        : this(reportId, instanceId, error, message, innerException)
    {
        this.ReportName = reportName;
    }
    #endregion

    #region Methods
    /// <summary>
    /// The report and instance this failure belongs to, for logs and notifications.
    /// </summary>
    /// <returns></returns>
    public string Describe()
        => $"Report '{this.ReportName}' (ID:{this.ReportId}){(this.InstanceId.HasValue ? $", Instance:{this.InstanceId}" : "")}";
    #endregion

}
