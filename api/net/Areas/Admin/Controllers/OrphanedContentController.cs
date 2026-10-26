using Microsoft.AspNetCore.Mvc;
using TNO.API.Helpers;
using TNO.Keycloak;
using TNO.DAL;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>Administrator-only diagnosis and repair of content missing from either store.</summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/orphaned-content")]
[Route("api/[area]/orphaned-content")]
[Route("v{version:apiVersion}/[area]/orphaned-content")]
[Route("[area]/orphaned-content")]
public class OrphanedContentController(OrphanedContentService service, OrphanScanService scans, IIndexRequestSender sender, TNOContext context) : ControllerBase
{
    /// <summary>Get the latest persisted scan, including an active scan started by another administrator.</summary>
    [HttpGet]
    public async Task<IActionResult> Latest(CancellationToken cancellationToken)
        => Ok(await scans.LatestAsync(cancellationToken));

    /// <summary>Start one background scan across all API replicas.</summary>
    [HttpPost("scans")]
    public async Task<IActionResult> Start(StartOrphanScan request, CancellationToken cancellationToken)
        => Accepted(await scans.StartAsync(request, cancellationToken));

    /// <summary>Request cancellation of an active scan at its next batch boundary.</summary>
    /// <param name="scanId">The persisted scan to cancel.</param>
    /// <param name="cancellationToken">Cancels the HTTP request while recording the cancellation request.</param>
    /// <returns>An accepted response after the cancellation request has been recorded.</returns>
    [HttpPost("scans/{scanId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid scanId, CancellationToken cancellationToken)
    {
        await scans.CancelAsync(scanId, cancellationToken);
        return Accepted(new { message = "Cancellation requested." });
    }

    /// <summary>Return a bounded page of confirmed results after or before a content ID, or the last page.</summary>
    [HttpGet("scans/{scanId:guid}/results")]
    public Task<OrphanScanResults> Results(Guid scanId, long after = 0, int limit = 100, CancellationToken cancellationToken = default, long? before = null, bool last = false)
        => scans.ResultsAsync(scanId, after, limit, cancellationToken, before, last);

    /// <summary>Send a database-only item through the normal indexing workflow.</summary>
    [HttpPost("{id:long}/index")]
    public async Task<IActionResult> Index(long id, string index = "content", Guid? scanId = null, CancellationToken cancellationToken = default)
    {
        await service.IndexAsync(id, index, cancellationToken);
        // Record queued only after Kafka acknowledges it. The action filter then has no requests left.
        await sender.SendAsync(context);
        await scans.MarkResolvedAsync(scanId, id, "Indexing queued", cancellationToken);
        return Accepted(new { message = "Indexing queued. Scan again after the indexer processes the request." });
    }

    /// <summary>Restore an Elasticsearch-only item to PostgreSQL using the original ID.</summary>
    [HttpPost("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id, string index = "content", Guid? scanId = null, CancellationToken cancellationToken = default)
    {
        await service.RestoreAsync(id, index, cancellationToken);
        await scans.MarkResolvedAsync(scanId, id, "Restored", cancellationToken);
        return Ok(new { message = "Content restored to PostgreSQL. Search refresh queued." });
    }

    /// <summary>Delete a current orphan or a previously synced result and clean up its search copies.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, string side, string index = "content", Guid? scanId = null, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, side, index, cancellationToken, scanId);
        await sender.SendAsync(context);
        await scans.MarkResolvedAsync(scanId, id, "Deleted", cancellationToken);
        return Ok(new { message = "Content deleted. Any queued search cleanup will be processed by the indexer." });
    }
}
