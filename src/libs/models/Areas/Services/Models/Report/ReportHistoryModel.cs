namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// Historical story input for report analysis, without saved HTML or delivery details.
/// Stories and section definitions are transferred once; instance references preserve membership.
/// </summary>
public class ReportHistoryModel
{
    /// <summary>get/set - Prior sent instances, newest first.</summary>
    public ReportHistoryInstanceModel[] Instances { get; set; } = [];

    /// <summary>get/set - The report's section definitions, shared by all instances.</summary>
    public ReportSectionModel[] Sections { get; set; } = [];

    /// <summary>get/set - Unique stories, retaining text and custom versions for AI input.</summary>
    public ContentModel[] Content { get; set; } = [];
}

/// <summary>
/// A historical instance's identity, publication date and ordered story references.
/// </summary>
public class ReportHistoryInstanceModel
{
    /// <summary>get/set - The historical instance ID.</summary>
    public long Id { get; set; }

    /// <summary>get/set - When this instance was published.</summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>get/set - Every story occurrence, including occurrences in multiple sections.</summary>
    public ReportHistoryContentReferenceModel[] Content { get; set; } = [];
}

/// <summary>
/// A story's position within one section of a historical instance.
/// </summary>
public class ReportHistoryContentReferenceModel
{
    /// <summary>get/set - The story ID in the shared content collection.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - The section's unique name.</summary>
    public string SectionName { get; set; } = "";

    /// <summary>get/set - The story's order in this section.</summary>
    public int SortOrder { get; set; }
}
