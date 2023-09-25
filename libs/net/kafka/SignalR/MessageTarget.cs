using System.ComponentModel;
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
}
