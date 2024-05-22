using System.Text.Json;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Admin.Models.ChartTemplate;

/// <summary>
/// ChartPreviewRequestModel class, provides a model to request a preview of a generated chart.
/// </summary>
public class ChartPreviewRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Chart settings.
    /// </summary>
    public ChartSectionSettingsModel Settings { get; set; } = new();

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
    /// get/set - Foreign key to a filter that will be used to populate the content for this chart.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - Foreign key to a linked report that will be used to populate the content for this chart.
    /// </summary>
    public int? LinkedReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to a folder that will be used to populate the content for this chart.
    /// </summary>
    public int? FolderId { get; set; }

    /// <summary>
    /// get/set - An array of content to be used to generate the JSON Data.
    /// </summary>
    public IEnumerable<TNO.API.Areas.Services.Models.Content.ContentModel>? Content { get; set; }

    /// <summary>
    /// get/set - The chart JSON data that will be sent to the Chart API.
    /// </summary>
    public JsonDocument? ChartData { get; set; }
    #endregion
}
