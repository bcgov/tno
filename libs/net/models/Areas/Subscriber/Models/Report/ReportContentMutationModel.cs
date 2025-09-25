using System;
using System.Collections.Generic;
using System.Linq;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportContentMutationModel class, describes the outcome of appending content to a report instance.
/// </summary>
public class ReportContentMutationModel
{
    #region Properties
    public int ReportId { get; set; }
    public long InstanceId { get; set; }
    public int? OwnerId { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime? PublishedOn { get; set; }
    public DateTime? SentOn { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Response { get; set; }
    public IEnumerable<ReportInstanceContentModel> Added { get; set; } = Enumerable.Empty<ReportInstanceContentModel>();
    public bool InstanceCreated { get; set; }
    #endregion
}
