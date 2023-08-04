using System.Text.Json;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Charts;

/// <summary>
/// ChartRequestModel class, provides a model that represents an report preview.
/// </summary>
public class ChartRequestModel
{
    #region Properties
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
    public IEnumerable<Reports.ContentModel>? Content { get; set; } = Array.Empty<Reports.ContentModel>();

    /// <summary>
    /// get/set - The chart JSON data that will be sent to the Chart API.
    /// </summary>
    public JsonDocument? ChartData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartRequestModel.
    /// </summary>
    public ChartRequestModel() { }

    /// <summary>
    /// Creates a new instance of an ChartRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="template"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <param name="content"></param>
    public ChartRequestModel(ChartSettingsModel settings, string template, string index, string? filter, IEnumerable<Reports.ContentModel>? content = null)
    {
        this.Settings = settings;
        this.Template = template;
        this.Index = index;
        this.Filter = filter != null ? JsonDocument.Parse(filter) : null;
        this.Content = content;
    }

    /// <summary>
    /// Creates a new instance of an ChartRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="template"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <param name="content"></param>
    public ChartRequestModel(ChartSettingsModel settings, string template, string index, JsonDocument? filter, IEnumerable<Reports.ContentModel>? content = null)
    {
        this.Settings = settings;
        this.Template = template;
        this.Index = index;
        this.Filter = filter;
        this.Content = content;
    }

    /// <summary>
    /// Creates a new instance of an ChartRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="template"></param>
    /// <param name="content"></param>
    public ChartRequestModel(ChartSettingsModel settings, string template, IEnumerable<Reports.ContentModel>? content = null)
    {
        this.Settings = settings;
        this.Template = template;
        this.Content = content;
    }
    #endregion
}
