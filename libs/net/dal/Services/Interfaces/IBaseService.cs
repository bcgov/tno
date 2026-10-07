using System.Security.Claims;

namespace TNO.DAL.Services;

public interface IBaseService
{
    ClaimsPrincipal Principal { get; }

    IServiceProvider Services { get; }

    int CommitTransaction();

    void ClearChangeTracker();

    /// <summary>
    /// Request indexing of saved content. The request is recorded by the next save on this context,
    /// in the same transaction as the change it follows, and sent once it commits.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="action"></param>
    /// <param name="requestorId"></param>
    /// <param name="reason"></param>
    void RequestIndex(long contentId, Entities.IndexRequestAction? action = null, int? requestorId = null, string reason = TNOContext.IndexReasonLifecycle);

    /// <summary>
    /// Request indexing of content being added or changed. The request is recorded by the next save
    /// on this context (new content once it has a key), and sent once it commits.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="action">The action, or null to derive it from the content's status.</param>
    /// <param name="requestorId"></param>
    /// <param name="reason"></param>
    void RequestIndex(Entities.Content content, Entities.IndexRequestAction? action = null, int? requestorId = null, string reason = TNOContext.IndexReasonLifecycle);
}
