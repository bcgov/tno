using TNO.API.Models.Settings;
using TNO.Core.Extensions;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportEngineContentModel class, provides a model to pass to the razor engine for reports.
/// </summary>
public class ReportEngineContentModel : BaseTemplateModel<IEnumerable<ContentModel>>
{
    #region Properties
    /// <summary>
    /// get/set - A dictionary with each section.
    /// </summary>
    public Dictionary<string, ReportSectionModel> Sections { get; set; } = new();

    /// <summary>
    /// get/set - The report settings.
    /// </summary>
    public ReportSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The user who owns the report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether the template should only include a link to the website.
    /// </summary>
    public bool ViewOnWebOnly { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel.
    /// </summary>
    /// <param name="report"></param>
    public ReportEngineContentModel() : base(Array.Empty<ContentModel>())
    {
    }

    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="content"></param>
    public ReportEngineContentModel(API.Areas.Services.Models.Report.ReportModel report, IEnumerable<ContentModel> content)
        : base(content)
    {
        this.Settings = report.Settings;
        this.OwnerId = report.OwnerId;
    }

    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sections"></param>
    /// <param name="uploadPath"></param>
    public ReportEngineContentModel(API.Areas.Services.Models.Report.ReportModel report, Dictionary<string, ReportSectionModel> sections, string? uploadPath = null)
        : base(Array.Empty<ContentModel>())
    {
        this.Sections = sections;
        this.Settings = report.Settings;
        this.OwnerId = report.OwnerId;

        // Reference all section content in the root Content collection.
        this.Content = sections.SelectMany(s => s.Value.Content).DistinctBy(c => c.Id);

        // Convert any images to base64 and include them in the email.
        if (!string.IsNullOrWhiteSpace(uploadPath) && this.Content.Any())
            sections.SelectMany(s => s.Value.Content).Where(c => c.ContentType == Entities.ContentType.Image).ForEach(c =>
            {
                c.ImageContent = GetImageContent(uploadPath, c.FileReferences.FirstOrDefault()?.Path);
            });
    }
    #endregion
}
