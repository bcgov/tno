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
}
