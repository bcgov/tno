using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.DAL.Services;

namespace TNO.API.Helpers;

/// <summary>
/// IReportAISectionTestStore interface, holds the outcome of AI section test runs for the user who
/// requested them, for as long as the workbench needs them.
/// </summary>
public interface IReportAISectionTestStore
{
    /// <summary>
    /// Record that a run was requested, so its outcome can be fetched while it is pending. The
    /// requestor's earlier runs of the section are dropped, and stale runs are purged.
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
    /// <returns>The run, or null when it is unknown or has been removed.</returns>
    ReportAISectionTestResultModel? Find(Guid testId);

    /// <summary>
    /// Find the run the user still has pending for the section, if any. A run pending for longer
    /// than <see cref="ReportAISectionTestStore.PendingLifetime"/> is treated as lost.
    /// </summary>
    /// <param name="requestorId"></param>
    /// <param name="sectionId"></param>
    /// <returns></returns>
    ReportAISectionTestResultModel? FindPending(int requestorId, int sectionId);

    /// <summary>
    /// Remove a run the requestor no longer needs.
    /// </summary>
    /// <param name="testId"></param>
    /// <returns>Whether a run was removed.</returns>
    bool Delete(Guid testId);
}

/// <summary>
/// ReportAISectionTestStore class, keeps AI section test runs in the database. The browser talks to
/// one API deployment and the reporting service posts to another, so the run must be visible to
/// both; nothing about it is held in process memory.
/// </summary>
public class ReportAISectionTestStore : IReportAISectionTestStore
{
    #region Variables
    /// <summary>
    /// How long a run may stay pending before it is treated as lost and another may start.
    /// </summary>
    public static readonly TimeSpan PendingLifetime = TimeSpan.FromMinutes(30);

    /// <summary>
    /// How long any run is kept before it is purged, whether or not the workbench removed it.
    /// </summary>
    public static readonly TimeSpan Retention = TimeSpan.FromHours(1);

    private readonly IReportAITestService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAISectionTestStore object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public ReportAISectionTestStore(IReportAITestService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public ReportAISectionTestResultModel Start(int reportId, long instanceId, int sectionId, int requestorId)
    {
        _service.Purge(DateTime.UtcNow - Retention);
        var test = _service.Start(new Entities.ReportAITest(Guid.NewGuid(), reportId, instanceId, sectionId, requestorId));
        return new ReportAISectionTestResultModel(test, _serializerOptions);
    }

    /// <inheritdoc/>
    public ReportAISectionTestResultModel Complete(ReportAISectionTestResultModel result)
    {
        var pending = _service.FindById(result.TestId);
        result.RequestorId ??= pending?.RequestorId;
        if (pending != null) result.CreatedOn = pending.CreatedOn;
        result.CompletedOn ??= DateTime.UtcNow;
        var stored = _service.Complete(result.ToEntity(_serializerOptions));
        return new ReportAISectionTestResultModel(stored, _serializerOptions);
    }

    /// <inheritdoc/>
    public ReportAISectionTestResultModel? Find(Guid testId)
    {
        var test = _service.FindById(testId);
        return test == null ? null : new ReportAISectionTestResultModel(test, _serializerOptions);
    }

    /// <inheritdoc/>
    public ReportAISectionTestResultModel? FindPending(int requestorId, int sectionId)
    {
        var test = _service.FindPending(requestorId, sectionId, DateTime.UtcNow - PendingLifetime);
        return test == null ? null : new ReportAISectionTestResultModel(test, _serializerOptions);
    }

    /// <inheritdoc/>
    public bool Delete(Guid testId) => _service.Delete(testId);
    #endregion
}
