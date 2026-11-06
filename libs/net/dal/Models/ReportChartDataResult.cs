using TNO.API.Models.Charts;

namespace TNO.DAL.Models;

/// <summary>
/// ReportChartDataResult class, provides the computed data of one chart.
/// </summary>
public class ReportChartDataResult
{
    #region Properties
    /// <summary>
    /// get/set - The Chart.js data.
    /// </summary>
    public ChartDataModel Data { get; set; } = new();

    /// <summary>
    /// get/set - The number of stories counted across all source sections.
    /// </summary>
    public long DocCount { get; set; }

    /// <summary>
    /// get/set - Whether a bucket list was longer than the aggregation returns.
    /// </summary>
    public bool Truncated { get; set; }

    /// <summary>
    /// get/set - Messages for the caller to log.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    #endregion
}
