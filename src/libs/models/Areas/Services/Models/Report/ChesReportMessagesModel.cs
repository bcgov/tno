namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// ChesReportMessagesModel class, provides a model to pass CHES report response message Ids.
/// </summary>
public class ChesReportMessagesModel
{
    /// <summary>
    /// get/set - The report type.
    /// </summary>
    public Entities.ReportType ReportType { get; set; }

    /// <summary>
    /// get/set - The format of the report.
    /// </summary>
    public Entities.ReportDistributionFormat Format { get; set; }

    /// <summary>
    /// get/set - The primary key to the report.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The primary key to the report instance.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The primary key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - When the report was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The status of this CHES request.
    /// </summary>
    public Entities.ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - The CHES message Ids.
    /// </summary>
    public IEnumerable<Guid> MessageIds { get; set; } = [];
}
