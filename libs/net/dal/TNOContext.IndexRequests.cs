using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using TNO.Entities;

namespace TNO.DAL;

/// <summary>
/// SavedIndexRequest record, an index request recorded by a save, to send to the indexer once the
/// save has committed.
/// </summary>
/// <param name="ContentId">The content.</param>
/// <param name="Action">What the indexer does.</param>
/// <param name="ProjectionRevision">The content's projection revision after this request.</param>
/// <param name="Reason">Why the content is indexed ('lifecycle', 'analysis').</param>
/// <param name="RequestorId">The user who requested it.</param>
public record SavedIndexRequest(long ContentId, IndexRequestAction Action, long ProjectionRevision, string Reason, int? RequestorId);

/// <summary>
/// TNOContext index requests. A caller requests indexing before it saves; the save increments the
/// content's projection revision in the same transaction as the change (the indexer versions
/// Elasticsearch writes with it), and keeps the request for the caller to send to Kafka once the
/// transaction commits. The API sends them before it responds, so a request fails when Kafka does.
/// </summary>
public partial class TNOContext
{
    #region Variables
    /// <summary>The reason for index requests that follow ordinary content changes.</summary>
    public const string IndexReasonLifecycle = "lifecycle";

    /// <summary>The reason for index requests that follow accepted analysis.</summary>
    public const string IndexReasonAnalysis = "analysis";

    private readonly List<PendingIndexRequest> _pendingIndexRequests = new();
    private readonly List<(SavedIndexRequest Request, DbTransaction? Transaction)> _savedIndexRequests = new();
    private readonly HashSet<DbTransaction> _committedTransactions = new();

    /// <summary>Tells each context when its transactions commit (for index and analysis requests).</summary>
    private static readonly IndexRequestTransactionInterceptor TransactionInterceptor = new();

    /// <summary>
    /// An index request waiting for the next save.
    /// </summary>
    /// <param name="Action">The action, or null to derive it from the content's saved status.</param>
    private record PendingIndexRequest(Content? Content, long ContentId, IndexRequestAction? Action, string Reason, int? RequestorId, long CurrentRevision);

    /// <summary>
    /// The content's projection revision and status after the save increments it.
    /// </summary>
    private class ProjectionRow
    {
        public long ProjectionRevision { get; set; }
        public ContentStatus Status { get; set; }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Request indexing of content that is being added or changed; recorded by the next save. New
    /// content is recorded once the save gives it a key.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="action"></param>
    /// <param name="reason"></param>
    /// <param name="requestorId"></param>
    public void RequestIndex(Content content, IndexRequestAction action, string reason = IndexReasonLifecycle, int? requestorId = null)
    {
        _pendingIndexRequests.Add(new PendingIndexRequest(content, content.Id, action, reason, requestorId, content.ProjectionRevision));
    }

    /// <summary>
    /// Request indexing of saved content by ID; recorded by the next save. When 'action' is null
    /// it is derived from the content's status as saved. A delete carries the revision the content
    /// has now, plus one, since the row is gone when the save records it.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="action"></param>
    /// <param name="reason"></param>
    /// <param name="requestorId"></param>
    public void RequestIndex(long contentId, IndexRequestAction? action = null, string reason = IndexReasonLifecycle, int? requestorId = null)
    {
        var currentRevision = 0L;
        if (action == IndexRequestAction.Delete)
            currentRevision = this.Database.SqlQueryRaw<long>(
                    @"SELECT projection_revision AS ""Value"" FROM public.content WHERE id = {0}", contentId)
                .AsEnumerable()
                .FirstOrDefault();
        _pendingIndexRequests.Add(new PendingIndexRequest(null, contentId, action, reason, requestorId, currentRevision));
    }

    /// <summary>
    /// The index action for content in its current state: published content goes to the published
    /// index as well.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static IndexRequestAction GetIndexAction(ContentStatus status)
        => status switch
        {
            ContentStatus.Publish or ContentStatus.Published => IndexRequestAction.Publish,
            ContentStatus.Unpublish => IndexRequestAction.Unpublish,
            _ => IndexRequestAction.Index,
        };

    /// <summary>
    /// Take the index requests saved since the last call whose transaction committed, to send to the
    /// indexer. Requests whose transaction rolled back (or is still open) are discarded.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<SavedIndexRequest> TakeIndexRequests()
    {
        var requests = _savedIndexRequests
            .Where(r => r.Transaction == null || _committedTransactions.Contains(r.Transaction))
            .Select(r => r.Request)
            .ToArray();
        _savedIndexRequests.Clear();
        _committedTransactions.Clear();
        return requests;
    }

    private void OnTransactionCommitted(DbTransaction transaction)
    {
        if (_savedIndexRequests.Any(r => r.Transaction == transaction)) _committedTransactions.Add(transaction);
        OnAnalysisTransactionCommitted(transaction);
    }

    /// <summary>
    /// IndexRequestTransactionInterceptor class, tells the context a transaction committed.
    /// </summary>
    private sealed class IndexRequestTransactionInterceptor : DbTransactionInterceptor
    {
        public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        {
            (eventData.Context as TNOContext)?.OnTransactionCommitted(transaction);
            base.TransactionCommitted(transaction, eventData);
        }

        public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            (eventData.Context as TNOContext)?.OnTransactionCommitted(transaction);
            return base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
        }
    }

    /// <summary>
    /// Increment the projection revision of each requested content item, in the save's transaction,
    /// and keep the requests to send. Called after the entity changes are saved, so new content has
    /// its key.
    /// </summary>
    private void RecordIndexRequests()
    {
        if (_pendingIndexRequests.Count == 0) return;
        var requests = _pendingIndexRequests.ToArray();
        _pendingIndexRequests.Clear();

        foreach (var request in requests)
        {
            var contentId = request.Content?.Id ?? request.ContentId;
            if (contentId == 0) continue;

            // Raw SQL, so the content's concurrency version is not changed by indexing.
            var row = this.Database.SqlQueryRaw<ProjectionRow>(
                    @"UPDATE public.content SET projection_revision = projection_revision + 1 WHERE id = {0} RETURNING projection_revision AS ""ProjectionRevision"", status AS ""Status""", contentId)
                .AsEnumerable()
                .FirstOrDefault();
            // A deleted content item has no row; its delete carries the next revision.
            var projectionRevision = row?.ProjectionRevision ?? request.CurrentRevision + 1;
            if (request.Content != null && row != null) request.Content.ProjectionRevision = row.ProjectionRevision;
            var action = request.Action
                ?? (row != null ? GetIndexAction(row.Status) : request.Content != null ? GetIndexAction(request.Content.Status) : IndexRequestAction.Delete);

            _savedIndexRequests.Add((new SavedIndexRequest(contentId, action, projectionRevision, request.Reason, request.RequestorId), this.Database.CurrentTransaction?.GetDbTransaction()));
        }
    }
    #endregion
}
