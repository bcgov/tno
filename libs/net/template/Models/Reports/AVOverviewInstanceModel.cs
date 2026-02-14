using System.Text.Json;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Reports;

public class AVOverviewInstanceModel
{
    #region Properties
    // <summary>
    // get/set - The template type.
    // </summary>
    public Entities.AVOverviewTemplateType TemplateType { get; set; }

    // <summary>
    // get/set - The published on date.
    // </summary>
    public DateTime PublishedOn { get; set; }

    /// <summary>
    /// get/set - An array of sections in this instance.
    /// </summary>
    public IEnumerable<AVOverviewSectionModel> Sections { get; set; } = Array.Empty<AVOverviewSectionModel>();

    /// <summary>
    /// get/set - Report settings.
    /// </summary>
    public AVOverviewSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The report status.
    /// </summary>
    public Entities.ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - The compiled subject of the report.
    /// Used to recreate the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The compiled body of the report.
    /// Used to recreate the report.
    /// </summary>
    public string Body { get; set; } = "";

    // <summary>
    // get/set - The response.
    // </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel.
    /// </summary>
    public AVOverviewInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewInstanceModel(Entities.AVOverviewInstance entity)
    {
        this.TemplateType = entity.TemplateType;
        this.PublishedOn = entity.PublishedOn;
        this.Sections = entity.Sections.Select(s => new AVOverviewSectionModel(s)).OrderBy(s => s.SortOrder).ToArray();
        this.Status = entity.Status;
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.Response = entity.Response;
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewInstanceModel(TNO.API.Areas.Editor.Models.AVOverview.AVOverviewInstanceModel model)
    {
        this.TemplateType = model.TemplateType;
        this.PublishedOn = model.PublishedOn;
        this.Sections = model.Sections.Select(s => new AVOverviewSectionModel(s)).OrderBy(s => s.SortOrder).ToArray();
        this.Status = model.Status;
        this.Subject = model.Subject;
        this.Body = model.Body;
        this.Response = model.Response;
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewInstanceModel(TNO.API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel model)
    {
        this.TemplateType = model.TemplateType;
        this.PublishedOn = model.PublishedOn;
        this.Sections = model.Sections.Select(s => new AVOverviewSectionModel(s)).OrderBy(s => s.SortOrder).ToArray();
        this.Status = model.Status;
        this.Subject = model.Subject;
        this.Body = model.Body;
        this.Response = model.Response;
    }
    #endregion
}
