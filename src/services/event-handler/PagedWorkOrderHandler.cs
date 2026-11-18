using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.EventHandler.Config;
using WorkOrderModel = TNO.API.Areas.Services.Models.WorkOrder.WorkOrderModel;

namespace TNO.Services.EventHandler;

/// <summary>
/// WorkOrderPage record, the outcome of one page of a work order.
/// </summary>
/// <typeparam name="TConfiguration"></typeparam>
/// <param name="Update">Records the page's progress in the work order's configuration.</param>
/// <param name="IsLast">Whether the work order is finished.</param>
/// <param name="Summary">Logged once the page's progress is saved.</param>
public record WorkOrderPage<TConfiguration>(Action<TConfiguration> Update, bool IsLast, string Summary);

/// <summary>
/// PagedWorkOrderHandler class, runs a work order a page at a time. Each work order message does one
/// page, saves the progress in the work order's configuration, and sends a message to continue, so no
/// message runs for long and a restarted Event Handler picks up from the saved progress.
///
/// A message carries the work order's version; a message for an older version is ignored, so only
/// one chain of messages works on a work order. A page that fails is thrown so the message is
/// received again; once it has failed 'RetryLimit' times the work order is marked failed. A
/// cancellation is never overwritten.
/// </summary>
/// <typeparam name="TConfiguration">The work order's configuration.</typeparam>
public abstract class PagedWorkOrderHandler<TConfiguration>
    where TConfiguration : class, new()
{
    #region Variables
    /// <summary>
    /// How the work order configuration is read and written.
    /// </summary>
    protected static readonly JsonSerializerOptions ConfigurationOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
    private readonly Dictionary<(long Id, long? Version), int> _failures = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The API.
    /// </summary>
    protected IApiService Api { get; }

    /// <summary>
    /// get - The Event Handler configuration.
    /// </summary>
    protected EventHandlerOptions Options { get; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a PagedWorkOrderHandler object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    protected PagedWorkOrderHandler(IApiService api, IOptions<EventHandlerOptions> options, ILogger logger)
    {
        this.Api = api;
        this.Options = options.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Do the work order's next page and continue, or finish.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task HandleAsync(WorkOrderRequestModel request)
    {
        var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
        if (workOrder == null)
        {
            this.Logger.LogWarning("Work order {id} does not exist", request.WorkOrderId);
            return;
        }
        if (workOrder.Status != WorkOrderStatus.Submitted && workOrder.Status != WorkOrderStatus.InProgress)
        {
            this.Logger.LogInformation("Work order {id} is {status}; nothing to do", workOrder.Id, workOrder.Status);
            return;
        }
        // Another message has continued the work order since this one was sent.
        if (request.Version.HasValue && workOrder.Version != request.Version)
        {
            this.Logger.LogDebug("Work order {id} message for version {version} is out of date (now {current})", workOrder.Id, request.Version, workOrder.Version);
            return;
        }

        var key = (workOrder.Id, workOrder.Version);
        try
        {
            var page = await ProcessPageAsync(workOrder, ReadConfiguration(workOrder));
            var saved = await SaveProgressAsync(workOrder.Id, page.Update, page.IsLast ? WorkOrderStatus.Completed : WorkOrderStatus.InProgress);
            this.Logger.LogInformation("Work order {id} {summary}{done}", workOrder.Id, page.Summary, page.IsLast ? "; complete" : "");

            if (saved?.Status == WorkOrderStatus.InProgress)
            {
                var result = await this.Api.SendMessageAsync(new WorkOrderRequestModel(saved.Id, saved.WorkType, request.Requestor, request.RequestedOn)
                {
                    RequestorId = request.RequestorId,
                    Version = saved.Version,
                });
                if (result == null) throw new InvalidOperationException($"Kafka did not accept the message to continue work order {saved.Id}");
            }
            _failures.Remove(key);
        }
        catch (Exception ex)
        {
            var failures = _failures.GetValueOrDefault(key) + 1;
            _failures[key] = failures;
            if (failures < Math.Max(1, this.Options.RetryLimit)) throw;

            // A stopped work order can be resumed or run again by an administrator.
            _failures.Remove(key);
            this.Logger.LogError(ex, "Work order {id} failed {failures} time(s) and is stopped", workOrder.Id, failures);
            await SaveProgressAsync(workOrder.Id, c => SetError(c, ex.Message), WorkOrderStatus.Failed);
        }
    }

    /// <summary>
    /// Do one page of the work order.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected abstract Task<WorkOrderPage<TConfiguration>> ProcessPageAsync(WorkOrderModel workOrder, TConfiguration configuration);

    /// <summary>
    /// Record why the work order stopped.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="error"></param>
    protected abstract void SetError(TConfiguration configuration, string error);

    /// <summary>
    /// Update the work order's configuration and status, retrying when another change saved first.
    /// A cancellation is never overwritten.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="update"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private Task<WorkOrderModel?> SaveProgressAsync(long id, Action<TConfiguration> update, WorkOrderStatus status)
    {
        return this.Api.HandleConcurrencyAsync(async () =>
        {
            var workOrder = await this.Api.FindWorkOrderAsync(id) ?? throw new NoContentException($"Work order {id} does not exist");
            var configuration = ReadConfiguration(workOrder);
            update(configuration);
            workOrder.Configuration = JsonSerializer.SerializeToElement(configuration, ConfigurationOptions).Deserialize<Dictionary<string, object>>(ConfigurationOptions) ?? new();
            if (workOrder.Status != WorkOrderStatus.Cancelled) workOrder.Status = status;
            return await this.Api.UpdateWorkOrderAsync(workOrder);
        });
    }

    /// <summary>
    /// The work order's configuration.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    private static TConfiguration ReadConfiguration(WorkOrderModel workOrder)
        => JsonSerializer.SerializeToElement(workOrder.Configuration, ConfigurationOptions).Deserialize<TConfiguration>(ConfigurationOptions) ?? new();
    #endregion
}
