using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.TopicScore;
using TNO.Core.Exceptions;
using TNO.Services.EventHandler.Config;
using WorkOrderModel = TNO.API.Areas.Services.Models.WorkOrder.WorkOrderModel;

namespace TNO.Services.EventHandler;

/// <summary>
/// TopicRescoreHandler class, runs a bulk topic rescore work order a page at a time: the API
/// recalculates each page's calculated scores and re-indexes the content whose score changed.
/// Rescoring a page twice changes nothing.
/// </summary>
public class TopicRescoreHandler : PagedWorkOrderHandler<TopicRescoreConfigurationModel>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicRescoreHandler object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TopicRescoreHandler(IApiService api, IOptions<EventHandlerOptions> options, ILogger<TopicRescoreHandler> logger)
        : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Rescore the next page.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override async Task<WorkOrderPage<TopicRescoreConfigurationModel>> ProcessPageAsync(WorkOrderModel workOrder, TopicRescoreConfigurationModel configuration)
    {
        var page = await this.Api.RescoreTopicsPageAsync(workOrder.Id, configuration.CheckpointContentId, Math.Clamp(this.Options.TopicRescorePageSize, 1, 1000))
            ?? throw new NoContentException($"Rescore work order {workOrder.Id} does not exist");

        return new WorkOrderPage<TopicRescoreConfigurationModel>(c =>
        {
            c.CheckpointContentId = page.LastContentId;
            c.Processed += page.Processed;
            c.Changed += page.Changed;
            c.Failed += page.Failed;
            if (page.Error != null) c.Error = page.Error;
        }, page.IsLast, $"rescored {page.Processed} content item(s), {page.Changed} changed, through content {page.LastContentId}");
    }

    /// <summary>
    /// Record why the rescore stopped.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="error"></param>
    protected override void SetError(TopicRescoreConfigurationModel configuration, string error) => configuration.Error = error;
    #endregion
}
