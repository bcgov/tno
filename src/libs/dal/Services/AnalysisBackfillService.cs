using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Core.Exceptions;
using TNO.DAL.Analysis;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AnalysisBackfillService class, administrator-requested analysis of existing content in a date
/// range. A backfill is a work order: the Event Handler reads it a page at a time and sends each
/// content item to the analysis backfill topic, which the Content-Analysis service consumes apart
/// from new content, so reports never depend on, wait for, or create backfill work.
/// </summary>
public class AnalysisBackfillService : BaseService, IAnalysisBackfillService
{
    #region Variables
    private const int BatchSize = 500;

    /// <summary>
    /// How a backfill's configuration is stored in its work order.
    /// </summary>
    public static readonly JsonSerializerOptions ConfigurationOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisBackfillService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public AnalysisBackfillService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<AnalysisBackfillService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Content in the range, created no later than the high-water mark.
    /// </summary>
    private IQueryable<Content> InRange(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, DateTime? highWaterMark)
    {
        var query = this.Context.Contents.AsNoTracking();
        query = dateField == AnalysisBackfillDateField.PublishedOn
            ? query.Where(c => c.PublishedOn >= startOn && c.PublishedOn < endOn)
            : query.Where(c => c.CreatedOn >= startOn && c.CreatedOn < endOn);
        if (highWaterMark.HasValue) query = query.Where(c => c.CreatedOn <= highWaterMark);
        return query;
    }

    private static IQueryable<Content> Eligible(IQueryable<Content> query, ContentAnalysisSettings settings)
    {
        var mediaTypes = settings.ExcludedMediaTypeIds.ToArray();
        var sources = settings.ExcludedSourceIds.ToArray();
        return query.Where(c => !mediaTypes.Contains(c.MediaTypeId) && (c.SourceId == null || !sources.Contains(c.SourceId.Value)));
    }

    /// <summary>
    /// Count what a backfill would cover.
    /// </summary>
    public AnalysisBackfillPreview Preview(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, AnalysisBackfillMode mode)
    {
        if (endOn <= startOn) throw new ArgumentException("The end must be after the start.");
        var settings = ContentAnalysisSettings.Read(this.Context);
        var range = InRange(startOn, endOn, dateField, null);
        var eligible = Eligible(range, settings);
        var matching = eligible.Count();
        var excluded = range.Count() - matching;
        var missingPublicationDate = dateField == AnalysisBackfillDateField.PublishedOn
            ? this.Context.Contents.AsNoTracking().Count(c => c.PublishedOn == null && c.CreatedOn >= startOn && c.CreatedOn < endOn)
            : 0;

        // Current analysis whose input still matches is skipped unless the backfill is forced.
        var alreadyCurrent = 0;
        if (mode == AnalysisBackfillMode.MissingOrStale)
        {
            long after = 0;
            while (true)
            {
                var batch = eligible.Where(c => c.Id > after).OrderBy(c => c.Id).Take(BatchSize).ToArray();
                if (batch.Length == 0) break;
                after = batch[^1].Id;
                var hashes = CurrentHashes(batch);
                alreadyCurrent += batch.Count(c => hashes.TryGetValue(c.Id, out var hash) && hash.Analysis == hash.Input);
            }
        }
        return new AnalysisBackfillPreview(matching, excluded, missingPublicationDate, alreadyCurrent);
    }

    /// <summary>
    /// The current input hash and current analysis hash of each content item.
    /// </summary>
    private Dictionary<long, (string Input, string? Analysis)> CurrentHashes(IReadOnlyList<Content> contents)
    {
        var ids = contents.Select(c => c.Id).ToArray();
        var ownedSummaries = this.Context.ContentFieldOwnerships.AsNoTracking()
            .Where(o => ids.Contains(o.ContentId) && o.Field == ContentFieldOwnership.SummaryField && o.ValueKey == ""
                && (o.Owner != FieldOwner.Human || o.IsCleared))
            .Select(o => o.ContentId)
            .ToHashSet();
        var analyses = this.Context.ContentAnalyses.AsNoTracking()
            .Where(a => ids.Contains(a.ContentId) && a.IsCurrent)
            .Select(a => new { a.ContentId, a.InputHash })
            .ToDictionary(a => a.ContentId, a => a.InputHash);
        return contents.ToDictionary(
            c => c.Id,
            c => (AnalysisInput.ComputeHash(c, !ownedSummaries.Contains(c.Id)), analyses.TryGetValue(c.Id, out var hash) ? hash : null));
    }


    /// <summary>
    /// Record a new backfill work order, counting the eligible content in its range now.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The range is empty.</exception>
    public WorkOrder Add(AnalysisBackfillConfigurationModel configuration, int? requestorId)
    {
        if (configuration.EndOn <= configuration.StartOn) throw new ArgumentException("The end must be after the start.");
        configuration.HighWaterMark = DateTime.UtcNow;
        configuration.CheckpointContentId = 0;
        configuration.Scheduled = 0;
        configuration.AlreadyCurrent = 0;
        configuration.Error = null;
        var settings = ContentAnalysisSettings.Read(this.Context);
        configuration.Total = Eligible(InRange(configuration.StartOn, configuration.EndOn, configuration.DateField, configuration.HighWaterMark), settings).Count();

        var workOrder = new WorkOrder(
            WorkOrderType.ContentAnalysisBackfill,
            $"Content-Analysis backfill {configuration.StartOn:yyyy-MM-dd} to {configuration.EndOn:yyyy-MM-dd}",
            JsonSerializer.SerializeToDocument(configuration, ConfigurationOptions))
        {
            RequestorId = requestorId,
        };
        this.Context.WorkOrders.Add(workOrder);
        this.Context.CommitTransaction();
        return workOrder;
    }

    /// <summary>
    /// The backfill work order.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public WorkOrder? FindById(long id)
        => this.Context.WorkOrders.AsNoTracking().FirstOrDefault(w => w.Id == id && w.WorkType == WorkOrderType.ContentAnalysisBackfill);

    /// <summary>
    /// The most recent backfill work orders.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    public IEnumerable<WorkOrder> FindRecent(int qty = 20)
    {
        return this.Context.WorkOrders.AsNoTracking()
            .Where(w => w.WorkType == WorkOrderType.ContentAnalysisBackfill)
            .OrderByDescending(w => w.Id)
            .Take(Math.Clamp(qty, 1, 100))
            .ToArray();
    }

    /// <summary>
    /// Stop the backfill. Content already sent is still analyzed unless the Content-Analysis service
    /// sees the cancellation first.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException">The backfill does not exist.</exception>
    public WorkOrder Cancel(long id)
    {
        var workOrder = this.Context.WorkOrders.FirstOrDefault(w => w.Id == id && w.WorkType == WorkOrderType.ContentAnalysisBackfill) ?? throw new NoContentException("Backfill does not exist");
        if (workOrder.Status == WorkOrderStatus.Submitted || workOrder.Status == WorkOrderStatus.InProgress)
        {
            workOrder.Status = WorkOrderStatus.Cancelled;
            this.Context.CommitTransaction();
        }
        return workOrder;
    }

    /// <summary>
    /// Continue a cancelled or failed backfill from its checkpoint.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException">The backfill does not exist.</exception>
    /// <exception cref="InvalidOperationException">The backfill is not cancelled or failed.</exception>
    public WorkOrder Resume(long id)
    {
        var workOrder = this.Context.WorkOrders.FirstOrDefault(w => w.Id == id && w.WorkType == WorkOrderType.ContentAnalysisBackfill) ?? throw new NoContentException("Backfill does not exist");
        if (workOrder.Status != WorkOrderStatus.Cancelled && workOrder.Status != WorkOrderStatus.Failed)
            throw new InvalidOperationException("Only a cancelled or failed backfill can be resumed.");
        var configuration = ReadConfiguration(workOrder);
        configuration.Error = null;
        workOrder.Configuration = JsonSerializer.SerializeToDocument(configuration, ConfigurationOptions);
        workOrder.Status = WorkOrderStatus.Submitted;
        this.Context.CommitTransaction();
        return workOrder;
    }

    /// <summary>
    /// The next page of a backfill: eligible content in its range after the checkpoint, leaving out
    /// content whose analysis is current unless the backfill is forced.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="afterContentId"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public AnalysisBackfillPageModel FindPage(AnalysisBackfillConfigurationModel configuration, long afterContentId, int quantity = BatchSize)
    {
        var settings = ContentAnalysisSettings.Read(this.Context);
        var batch = Eligible(InRange(configuration.StartOn, configuration.EndOn, configuration.DateField, configuration.HighWaterMark), settings)
            .Where(c => c.Id > afterContentId)
            .OrderBy(c => c.Id)
            .Take(Math.Clamp(quantity, 1, 1000))
            .ToArray();
        if (batch.Length == 0) return new AnalysisBackfillPageModel() { LastContentId = afterContentId, IsLast = true };

        var hashes = CurrentHashes(batch);
        var items = new List<AnalysisBackfillItemModel>();
        var alreadyCurrent = 0;
        foreach (var content in batch)
        {
            var (input, analysis) = hashes[content.Id];
            if (configuration.Mode == AnalysisBackfillMode.MissingOrStale && analysis == input) alreadyCurrent++;
            else items.Add(new AnalysisBackfillItemModel() { ContentId = content.Id, InputHash = input });
        }
        return new AnalysisBackfillPageModel()
        {
            Items = items,
            AlreadyCurrent = alreadyCurrent,
            LastContentId = batch[^1].Id,
            IsLast = batch.Length < Math.Clamp(quantity, 1, 1000),
        };
    }

    /// <summary>
    /// A backfill work order's configuration.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public static AnalysisBackfillConfigurationModel ReadConfiguration(WorkOrder workOrder)
        => workOrder.Configuration.Deserialize<AnalysisBackfillConfigurationModel>(ConfigurationOptions) ?? new AnalysisBackfillConfigurationModel();
    #endregion
}
