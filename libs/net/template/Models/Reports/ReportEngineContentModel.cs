using System.Web;
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel.
    /// </summary>
    public ReportEngineContentModel() : base(Array.Empty<ContentModel>())
    {
    }

    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="settings"></param>
    public ReportEngineContentModel(IEnumerable<ContentModel> content, ReportSettingsModel settings)
        : base(content)
    {
        this.Settings = settings;
    }

    /// <summary>
    /// Creates a new instance of a ReportEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="settings"></param>
    /// <param name="uploadPath"></param>
    public ReportEngineContentModel(Dictionary<string, ReportSectionModel> sections, ReportSettingsModel settings, string? uploadPath = null)
        : base(Array.Empty<ContentModel>())
    {
        this.Sections = sections;
        this.Settings = settings;

        // Reference all section content in the root Content collection.
        this.Content = sections.SelectMany(s => s.Value.Content).DistinctBy(c => c.Id);

        // Convert any images to base64 and include them in the email.
        if (!string.IsNullOrWhiteSpace(uploadPath) && this.Content.Any())
            this.Content.Where(c => c.ContentType == Entities.ContentType.Image).ForEach(c =>
            {
                c.ImageContent = GetImageContent(uploadPath, c.FileReferences.FirstOrDefault()?.Path);
            });
    }
    #endregion
}
