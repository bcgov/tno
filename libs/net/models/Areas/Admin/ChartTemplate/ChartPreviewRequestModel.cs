using System.Text.Json;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Admin.Models.ChartTemplate;

/// <summary>
/// ChartPreviewRequestModel class, provides a model that represents an report preview.
/// </summary>
public class ChartPreviewRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Width of generated chart.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// get/set - Height of generated chart.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// get/set - Chart settings.
    /// </summary>
    public ChartSettingsModel Settings { get; set; } = new ChartSettingsModel();

    /// <summary>
    /// get/set - The razor template to generate JSON.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - The Elasticsearch index to search.
    /// </summary>
    public string? Index { get; set; }

    /// <summary>
    /// get/set - The Elasticsearch filter to find content.
    /// </summary>
    public JsonDocument? Filter { get; set; }

    /// <summary>
    /// get/set - An array of content to be used to generate the JSON Data.
    /// </summary>
    public IEnumerable<TNO.API.Areas.Editor.Models.Content.ContentModel>? Content { get; set; } = Array.Empty<TNO.API.Areas.Editor.Models.Content.ContentModel>();

    /// <summary>
    /// get/set - The chart JSON data that will be sent to the Chart API.
    /// </summary>
    public JsonDocument? ChartData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartPreviewRequestModel.
    /// </summary>
    public ChartPreviewRequestModel() { }

    /// <summary>
    /// Creates a new instance of an ChartPreviewRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="template"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    public ChartPreviewRequestModel(ChartSettingsModel settings, string template, string index, string filter)
    {
        this.Settings = settings;
        this.Template = template;
        this.Index = index;
        this.Filter = JsonDocument.Parse(filter);
    }

    /// <summary>
    /// Creates a new instance of an ChartPreviewRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="template"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    public ChartPreviewRequestModel(ChartSettingsModel settings, string template, string index, JsonDocument filter)
    {
        this.Settings = settings;
        this.Template = template;
        this.Index = index;
        this.Filter = filter;
    }
    #endregion
}
