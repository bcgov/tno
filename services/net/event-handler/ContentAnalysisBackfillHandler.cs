using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.EventHandler.Config;
using WorkOrderModel = TNO.API.Areas.Services.Models.WorkOrder.WorkOrderModel;

namespace TNO.Services.EventHandler;

/// <summary>
/// ContentAnalysisBackfillHandler class, runs a Content-Analysis backfill work order a page at a
/// time: each page of eligible content is sent to the analysis backfill topic. Sending a page twice is
/// harmless; Content-Analysis skips content whose analysis is already current.
/// </summary>
public class ContentAnalysisBackfillHandler : PagedWorkOrderHandler<AnalysisBackfillConfigurationModel>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisBackfillHandler object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAnalysisBackfillHandler(IApiService api, IOptions<EventHandlerOptions> options, ILogger<ContentAnalysisBackfillHandler> logger)
        : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send the backfill's next page of content for analysis.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override async Task<WorkOrderPage<AnalysisBackfillConfigurationModel>> ProcessPageAsync(WorkOrderModel workOrder, AnalysisBackfillConfigurationModel configuration)
    {
        var page = await this.Api.FindAnalysisBackfillPageAsync(workOrder.Id, configuration.CheckpointContentId, Math.Clamp(this.Options.AnalysisBackfillPageSize, 1, 1000))
            ?? throw new NoContentException($"Backfill work order {workOrder.Id} does not exist");
        var items = page.Items.ToArray();
        if (items.Length > 0)
        {
            var requestedOn = DateTime.UtcNow;
            await this.Api.SendAnalysisRequestsAsync(this.Options.AnalysisBackfillTopic, items.Select(i =>
                new AnalysisRequestModel(Guid.NewGuid().ToString("N"), i.ContentId, i.InputHash, AnalysisRequestReason.Backfill, configuration.Mode == AnalysisBackfillMode.Force, requestedOn)
                {
                    WorkOrderId = workOrder.Id,
                }));
        }

        return new WorkOrderPage<AnalysisBackfillConfigurationModel>(c =>
        {
            c.CheckpointContentId = page.LastContentId;
            c.Scheduled += items.Length;
            c.AlreadyCurrent += page.AlreadyCurrent;
        }, page.IsLast, $"sent {items.Length} content item(s) for analysis, through content {page.LastContentId}");
    }

    /// <summary>
    /// Record why the backfill stopped.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="error"></param>
    protected override void SetError(AnalysisBackfillConfigurationModel configuration, string error) => configuration.Error = error;
    #endregion
}
