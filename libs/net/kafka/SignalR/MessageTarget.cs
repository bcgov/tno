using System.ComponentModel.DataAnnotations;

namespace TNO.Kafka.SignalR;

public enum MessageTarget
{
    [Display(Name = "content-added")]
    ContentAdded = 0,

    [Display(Name = "content-updated")]
    ContentUpdated = 1,

    [Display(Name = "content-deleted")]
    ContentDeleted = 2,

    [Display(Name = "work-order")]
    WorkOrder = 3,

    [Display(Name = "content-action-updated")]
    ContentActionUpdated = 4,

    [Display(Name = "ingest-updated")]
    IngestUpdated = 5,

    [Display(Name = "ingest-deleted")]
    IngestDeleted = 6,

    [Display(Name = "report-status")]
    ReportStatus = 7,

    [Display(Name = "logout")]
    Logout = 8,

    [Display(Name = "system-message")]
    SystemMessage = 9,
}
