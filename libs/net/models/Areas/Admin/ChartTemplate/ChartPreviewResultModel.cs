using System.Text.Json;

namespace TNO.API.Areas.Admin.Models.ChartTemplate;

/// <summary>
/// ChartPreviewResultModel class, provides a model that represents an report preview.
/// </summary>
public class ChartPreviewResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The JSON result for the chart.
    /// </summary>
    public JsonDocument Json { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Elasticsearch results.
    /// </summary>
    public object? Results { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartPreviewResultModel.
    /// </summary>
    public ChartPreviewResultModel() { }

    /// <summary>
    /// Creates a new instance of an ChartPreviewResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="results"></param>
    public ChartPreviewResultModel(string json, object? results)
    {
        this.Json = JsonDocument.Parse(json);
        this.Results = results;
    }

    /// <summary>
    /// Creates a new instance of an ChartPreviewResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="results"></param>
    public ChartPreviewResultModel(JsonDocument json, object? results)
    {
        this.Json = json;
        this.Results = results;
    }
    #endregion
}
