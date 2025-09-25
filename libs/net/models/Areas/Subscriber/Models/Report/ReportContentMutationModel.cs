using System.Collections.Generic;
using System;
using System.Linq;
using TNO.DAL.Models;
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

    #region Constructors
    public ReportContentMutationModel() { }

    public ReportContentMutationModel(ReportContentMutation mutation)
    {
        ReportId = mutation.ReportId;
        InstanceId = mutation.Instance.Id;
        OwnerId = mutation.Instance.OwnerId;
        Status = mutation.Instance.Status;
        PublishedOn = mutation.Instance.PublishedOn;
        SentOn = mutation.Instance.SentOn;
        Subject = mutation.Instance.Subject;
        Body = mutation.Instance.Body;
        Response = mutation.Instance.Response;
        Added = mutation.Added.Select(c => new ReportInstanceContentModel(c)).ToArray();
        InstanceCreated = mutation.InstanceCreated;
    }
    #endregion
}
