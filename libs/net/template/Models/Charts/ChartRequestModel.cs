using System.Text.Json;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine.Models.Charts;

/// <summary>
/// ChartRequestModel class, provides a model that represents an report preview.
/// </summary>
public class ChartRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The chart template to use.
    /// </summary>
    public ChartEngineTemplateModel ChartTemplate { get; set; } = new();

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
    /// <param name="chartData"></param>
    public ChartRequestModel(ChartEngineTemplateModel model, JsonDocument? chartData = null)
    {
        this.ChartTemplate = model;
        this.ChartData = chartData;
    }

    /// <summary>
    /// Creates a new instance of an ChartRequestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="chartData"></param>
    public ChartRequestModel(ChartEngineTemplateModel model, string chartData)
    {
        this.ChartTemplate = model;
        this.ChartData = chartData != null ? JsonDocument.Parse(chartData) : null;
    }
    #endregion
}
