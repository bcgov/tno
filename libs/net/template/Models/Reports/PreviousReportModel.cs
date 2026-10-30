namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// PreviousReportModel class, one prior instance of a report, with the date it was published so an
/// AI section comparing several of them can tell them apart and order them in time.
/// </summary>
public class PreviousReportModel
{
    #region Properties
    /// <summary>
    /// get/set - The report instance this content came from.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - When the instance was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The instance's content, keyed by section name.
    /// </summary>
    public Dictionary<string, ReportSectionModel> Sections { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a PreviousReportModel.
    /// </summary>
    public PreviousReportModel() { }

    /// <summary>
    /// Creates a new instance of a PreviousReportModel, initializes with specified parameters.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="publishedOn"></param>
    /// <param name="sections"></param>
    public PreviousReportModel(long instanceId, DateTime? publishedOn, Dictionary<string, ReportSectionModel> sections)
    {
        this.InstanceId = instanceId;
        this.PublishedOn = publishedOn;
        this.Sections = sections;
    }
    #endregion

    /// <summary>
    /// Reconstruct the same AI history for API previews and the reporting worker.
    /// Each occurrence has its own ordering metadata while sharing the transferred story strings.
    /// </summary>
    /// <param name="history">The history response with unique stories.</param>
    /// <returns>Instances with every section occurrence preserved.</returns>
    public static PreviousReportModel[] FromHistory(API.Areas.Services.Models.Report.ReportHistoryModel history)
    {
        var stories = history.Content.ToDictionary(c => c.Id);
        return history.Instances.Select(instance => new PreviousReportModel(instance.Id, instance.PublishedOn,
            history.Sections.ToDictionary(section => section.Name, section => new ReportSectionModel(section,
                instance.Content.Where(c => c.SectionName == section.Name)
                    .Select(c => new ContentModel(stories[c.ContentId], c.SortOrder, c.SectionName, section.Settings.Label)))))).ToArray();
    }
}
