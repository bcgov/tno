using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TNO.DAL.Analysis;
using TNO.Entities;

namespace TNO.DAL;

/// <summary>
/// SavedAnalysisRequest record, an analysis request recorded by a save (or made directly), to send
/// to the Kafka analysis topic once the save has committed.
/// </summary>
/// <param name="RequestId">Identifies the request; its retries carry the same value.</param>
/// <param name="ContentId">The content.</param>
/// <param name="InputHash">Fingerprint of the analysis inputs as saved.</param>
/// <param name="Reason">Why the content is analyzed.</param>
/// <param name="Force">Analyze even when the current analysis is for the same input.</param>
/// <param name="RequestedOn">When the request was made.</param>
public record SavedAnalysisRequest(string RequestId, long ContentId, string InputHash, AnalysisRequestReason Reason, bool Force, DateTime RequestedOn)
{
    /// <summary>
    /// Create a new request.
    /// </summary>
    public static SavedAnalysisRequest Create(long contentId, string inputHash, AnalysisRequestReason reason, bool force = false)
        => new(Guid.NewGuid().ToString("N"), contentId, inputHash, reason, force, DateTime.UtcNow);
}

/// <summary>
/// TNOContext analysis requests. A save that adds content, or changes one of its analysis inputs,
/// records a request; the API sends the requests whose transaction committed to the Kafka analysis
/// topic before it responds. Kafka is the source of analysis work: nothing is queued in the database.
/// </summary>
public partial class TNOContext
{
    #region Variables
    private readonly List<(Content Content, string InputHash)> _pendingAnalysisRequests = new();
    private readonly List<(SavedAnalysisRequest Request, DbTransaction? Transaction)> _savedAnalysisRequests = new();
    #endregion

    #region Methods
    /// <summary>
    /// Request analysis of saved content, sent with the requests saves record.
    /// </summary>
    /// <param name="request"></param>
    public void RequestAnalysis(SavedAnalysisRequest request)
    {
        _savedAnalysisRequests.Add((request, this.Database.CurrentTransaction?.GetDbTransaction()));
    }

    /// <summary>
    /// Take the analysis requests made since the last call whose transaction committed. Requests whose
    /// transaction rolled back (or is still open) are discarded.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<SavedAnalysisRequest> TakeAnalysisRequests()
    {
        var requests = _savedAnalysisRequests.Where(r => r.Transaction == null).Select(r => r.Request).ToArray();
        _savedAnalysisRequests.Clear();
        return requests;
    }

    /// <summary>
    /// A committed transaction's analysis requests can be sent.
    /// </summary>
    /// <param name="transaction"></param>
    private void OnAnalysisTransactionCommitted(DbTransaction transaction)
    {
        for (var i = 0; i < _savedAnalysisRequests.Count; i++)
        {
            if (_savedAnalysisRequests[i].Transaction == transaction)
                _savedAnalysisRequests[i] = (_savedAnalysisRequests[i].Request, null);
        }
    }

    /// <summary>
    /// Before the save, note the content that is added or has a changed analysis input. Analysis's
    /// own changes are not sent back to it.
    /// </summary>
    private void CollectAnalysisRequests()
    {
        // A failed save leaves its changes tracked, so each save collects afresh.
        _pendingAnalysisRequests.Clear();
        if (this.ChangeOwner == FieldOwner.Analysis) return;

        foreach (var entry in ChangeTracker.Entries<Content>().ToArray())
        {
            var content = entry.Entity;
            if (entry.State == EntityState.Added)
            {
                _pendingAnalysisRequests.Add((content, AnalysisInput.ComputeHash(content, true)));
                continue;
            }
            if (entry.State != EntityState.Modified) continue;
            if (!AnalysisInput.Properties.Any(p => entry.Property(p).IsModified)
                && !ChangeTracker.Entries<ContentFieldOwnership>().Any(o => o.Entity.ContentId == content.Id && o.Entity.Field == ContentFieldOwnership.SummaryField && o.State != EntityState.Unchanged))
                continue;

            _pendingAnalysisRequests.Add((content, AnalysisInput.ComputeHash(content, IsSummaryHumanOwned(content.Id))));
        }
    }

    /// <summary>
    /// After the save, record the requests in the save's transaction; new content has its key now.
    /// </summary>
    private void RecordAnalysisRequests()
    {
        if (_pendingAnalysisRequests.Count == 0) return;
        var pending = _pendingAnalysisRequests.ToArray();
        _pendingAnalysisRequests.Clear();
        foreach (var (content, hash) in pending)
        {
            if (content.Id == 0) continue;
            RequestAnalysis(SavedAnalysisRequest.Create(content.Id, hash, AnalysisRequestReason.Lifecycle));
        }
    }
    #endregion
}
