using System.Text.Json;

namespace TNO.TemplateEngine.Models.Charts;

/// <summary>
/// ChartResultModel class, provides a model that represents an report preview.
/// </summary>
public class ChartResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The JSON result for the chart.
    /// </summary>
    public JsonDocument Json { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Elasticsearch results.
    /// </summary>
    public JsonDocument? Results { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartResultModel.
    /// </summary>
    public ChartResultModel() { }

    /// <summary>
    /// Creates a new instance of an ChartResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="results"></param>
    public ChartResultModel(string json, string? results = null)
    {
        this.Json = JsonDocument.Parse(json);
        this.Results = results != null ? JsonDocument.Parse(results) : null;
    }

    /// <summary>
    /// Creates a new instance of an ChartResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="results"></param>
    public ChartResultModel(JsonDocument json, JsonDocument? results = null)
    {
        this.Json = json;
        this.Results = results;
    }
    #endregion
}
