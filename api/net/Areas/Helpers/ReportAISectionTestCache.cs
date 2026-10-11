using Microsoft.Extensions.Caching.Memory;
using TNO.API.Areas.Services.Models.ReportAIResult;

namespace TNO.API.Helpers;

/// <summary>
/// IReportAISectionTestCache interface, holds the outcome of AI section test runs for the user who
/// requested them. Nothing is persisted: a run is kept in memory for a while after it is requested.
/// </summary>
public interface IReportAISectionTestCache
{
    /// <summary>
    /// Record that a run was requested, so its outcome can be fetched while it is pending.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instanceId"></param>
    /// <param name="sectionId"></param>
    /// <param name="requestorId"></param>
    /// <returns>The pending result.</returns>
    ReportAISectionTestResultModel Start(int reportId, long instanceId, int sectionId, int requestorId);

    /// <summary>
    /// Record the outcome of a run.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>The stored result, with the requestor of the pending entry when the outcome names none.</returns>
    ReportAISectionTestResultModel Complete(ReportAISectionTestResultModel result);

    /// <summary>
    /// Find a run.
    /// </summary>
    /// <param name="testId"></param>
    /// <returns>The run, or null when it is unknown or has expired.</returns>
    ReportAISectionTestResultModel? Find(Guid testId);

    /// <summary>
    /// Find the run the user still has pending for the section, if any.
    /// </summary>
    /// <param name="requestorId"></param>
    /// <param name="sectionId"></param>
    /// <returns></returns>
    ReportAISectionTestResultModel? FindPending(int requestorId, int sectionId);
}

/// <summary>
/// ReportAISectionTestCache class, keeps AI section test runs in the API's memory cache for
/// <see cref="Lifetime"/> after they are requested. The cache belongs to one API process, so every
/// request about a run must reach the same process (the API runs as a single replica).
/// </summary>
public class ReportAISectionTestCache : IReportAISectionTestCache
{
    #region Variables
    /// <summary>
    /// How long a run is kept after it is requested.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _cache;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAISectionTestCache object, initializes with specified parameters.
    /// </summary>
    /// <param name="cache"></param>
    public ReportAISectionTestCache(IMemoryCache cache)
    {
        _cache = cache;
    }
    #endregion

    #region Methods
    private static string Key(Guid testId) => $"ai-test:{testId}";
    private static string PendingKey(int requestorId, int sectionId) => $"ai-test:pending:{requestorId}:{sectionId}";

    /// <inheritdoc/>
    public ReportAISectionTestResultModel Start(int reportId, long instanceId, int sectionId, int requestorId)
    {
        var result = new ReportAISectionTestResultModel()
        {
            TestId = Guid.NewGuid(),
            ReportId = reportId,
            ReportInstanceId = instanceId,
            SectionId = sectionId,
            RequestorId = requestorId,
            Status = ReportAISectionTestStatus.Pending,
            CreatedOn = DateTime.UtcNow,
        };
        _cache.Set(Key(result.TestId), result, Lifetime);
        _cache.Set(PendingKey(requestorId, sectionId), result.TestId, Lifetime);
        return result;
    }

    /// <inheritdoc/>
    public ReportAISectionTestResultModel Complete(ReportAISectionTestResultModel result)
    {
        var pending = Find(result.TestId);
        result.RequestorId ??= pending?.RequestorId;
        if (pending != null) result.CreatedOn = pending.CreatedOn;
        result.CompletedOn ??= DateTime.UtcNow;
        // Keep the outcome for the rest of the run's lifetime, not a fresh lifetime.
        var remaining = result.CreatedOn + Lifetime - DateTime.UtcNow;
        _cache.Set(Key(result.TestId), result, remaining > TimeSpan.FromMinutes(1) ? remaining : TimeSpan.FromMinutes(1));
        if (result.RequestorId.HasValue) _cache.Remove(PendingKey(result.RequestorId.Value, result.SectionId));
        return result;
    }

    /// <inheritdoc/>
    public ReportAISectionTestResultModel? Find(Guid testId)
        => _cache.TryGetValue(Key(testId), out ReportAISectionTestResultModel? result) ? result : null;

    /// <inheritdoc/>
    public ReportAISectionTestResultModel? FindPending(int requestorId, int sectionId)
    {
        if (!_cache.TryGetValue(PendingKey(requestorId, sectionId), out Guid testId)) return null;
        var result = Find(testId);
        return result?.Status == ReportAISectionTestStatus.Pending ? result : null;
    }
    #endregion
}
