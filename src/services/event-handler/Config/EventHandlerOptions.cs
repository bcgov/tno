
using TNO.Services.Config;

namespace TNO.Services.EventHandler.Config;

/// <summary>
/// EventHandlerOptions class, configuration options for event handler service
/// </summary>
public class EventHandlerOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - A comma separated list of work order topics to consume. Work orders have their own
    /// consumer, so a long-running work order never delays event schedules.
    /// </summary>
    public string WorkOrderTopics { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic a Content-Analysis backfill sends content to.
    /// </summary>
    public string AnalysisBackfillTopic { get; set; } = "analysis-backfill";

    /// <summary>
    /// get/set - Content items a Content-Analysis backfill sends per work order message.
    /// </summary>
    public int AnalysisBackfillPageSize { get; set; } = 500;

    /// <summary>
    /// get/set - Content items a bulk topic rescore recalculates per work order message.
    /// </summary>
    public int TopicRescorePageSize { get; set; } = 200;
    #endregion
}
