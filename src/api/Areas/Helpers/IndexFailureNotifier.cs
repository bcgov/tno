using System.Net;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.DAL;

namespace TNO.API.Helpers;

/// <summary>Notify configured support when committed database changes could not reach the indexer.</summary>
public interface IIndexFailureNotifier
{
    /// <summary>Send an error alert without replacing the original failure if email delivery also fails.</summary>
    Task NotifyAsync(IEnumerable<SavedIndexRequest> requests, Exception error);
}

/// <summary>Uses CHES and the same Service:EmailTo/SendEmailOnFailure settings as service error alerts.</summary>
public class IndexFailureNotifier(IChesService ches, IOptions<ChesOptions> options,
    IConfiguration configuration, ILogger<IndexFailureNotifier> logger) : IIndexFailureNotifier
{
    /// <inheritdoc />
    public async Task NotifyAsync(IEnumerable<SavedIndexRequest> requests, Exception error)
    {
        if (!configuration.GetValue("Service:SendEmailOnFailure", true)) return;
        var recipients = (configuration["Service:EmailTo"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (recipients.Length == 0)
        {
            logger.LogWarning("Index failure email cannot be sent: Service:EmailTo is not configured.");
            return;
        }
        try
        {
            var changes = String.Join("\n", requests.Select(r => $"Content {r.ContentId}: {r.Action}, revision {r.ProjectionRevision}"));
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            await ches.SendEmailAsync(new EmailModel(options.Value.From, recipients,
                $"{environment} - MMI API - Index request failed",
                "<p>Database changes committed, but their index requests were not delivered. Review Orphaned Content in the Editor.</p>" +
                $"<pre>{WebUtility.HtmlEncode(changes)}\n{WebUtility.HtmlEncode(error.ToString())}</pre>"));
        }
        catch (Exception)
        {
            // CHES exceptions can include recipients or the email body. Keep these out of application logs.
            logger.LogError("Failed to send index failure email to support. Check email service health and configuration.");
        }
    }
}
