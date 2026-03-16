using System.Text.Json;

namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// SmtpReportMessagesModel class, provides a model to pass SMTP report response message Ids.
/// </summary>
public class SmtpReportMessagesModel
{
    #region Properties
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
    public int? UserId { get; set; }

    /// <summary>
    /// get/set - When the report was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The status of this CHES request.
    /// </summary>
    public Entities.ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - The mail response that was saved for this report.
    /// If UserId is nul, this is the response for the whole report which will normally include two arrays for the link and full text report responses.
    /// If UserId is not null, this is the response for the individual user report instances.
    /// </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");
    #endregion
}
