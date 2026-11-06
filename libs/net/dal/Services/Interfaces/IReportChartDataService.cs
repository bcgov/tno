using System.Text.Json;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// IReportChartDataService interface, provides chart data from Elasticsearch aggregations, so a
/// chart costs one number per bucket instead of one object per story.
/// </summary>
public interface IReportChartDataService : IBaseService
{
    /// <summary>
    /// get - Whether byline charts can aggregate, which requires 'byline.keyword' in the index.
    /// </summary>
    bool ByLineAggregationEnabled { get; }

    /// <summary>
    /// Compute the data of one chart with the built-in converter.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The chart groups by byline and byline aggregation is off.</exception>
    Task<ReportChartDataResult> GetChartDataAsync(ReportChartDataRequest request);

    /// <summary>
    /// Run a chart template's own 'aggs' block, once per source section.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="aggs"></param>
    /// <returns>The aggregations by source section name (by "section:name" when the block declares more than one).</returns>
    Task<Dictionary<string, Elastic.Models.AggregationRootModel>> AggregateAsync(ReportChartDataRequest request, JsonDocument aggs);

    /// <summary>
    /// Build the request for a chart in a section, resolving the section's source to its stories.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="section"></param>
    /// <param name="chart"></param>
    /// <param name="source"></param>
    /// <param name="instanceId">The instance whose stories 'InstanceIds' uses.</param>
    /// <param name="contentBySection">The stories by section, when there is no instance to read them from.</param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    ReportChartDataRequest BuildRequest(
        Report report,
        ReportSection section,
        ReportSectionChartTemplate chart,
        ReportChartSource source,
        long? instanceId,
        IDictionary<string, long[]>? contentBySection,
        int? requestorId);

    /// <summary>
    /// Compute the data of every chart in the report's media analytics sections that can aggregate.
    /// Nothing is saved. A chart that cannot aggregate has no row and renders from stories.
    /// </summary>
    /// <param name="report">The report, with its sections and their chart templates.</param>
    /// <param name="instanceId">The instance whose stories 'InstanceIds' uses.</param>
    /// <param name="contentBySection">The stories by section, when there is no instance to read them from.</param>
    /// <param name="requestorId"></param>
    /// <param name="sectionId">Limit the work to this section.</param>
    /// <param name="renderBuckets">Renders a chart template over its buckets. Razor is not available here, so bucket mode charts have no row without it.</param>
    /// <returns></returns>
    Task<IEnumerable<ReportInstanceChart>> ComputeChartsAsync(
        Report report,
        long? instanceId,
        IDictionary<string, long[]>? contentBySection,
        int? requestorId,
        int? sectionId = null,
        Func<ReportSection, ReportSectionChartTemplate, Dictionary<string, Elastic.Models.AggregationRootModel>, Task<JsonDocument>>? renderBuckets = null);
}
