using System.Security.Cryptography;
using System.Text;
using TNO.Entities;

namespace TNO.DAL.Analysis;

/// <summary>
/// AnalysisInput class, the fingerprint of the values Content-Analysis reads. Workflow changes
/// (status, actions, publication) and values analysis itself populated do not change it.
/// </summary>
public static class AnalysisInput
{
    /// <summary>
    /// The fingerprint version; changing what is fingerprinted changes it.
    /// </summary>
    public const string Version = "1";

    /// <summary>
    /// The content properties that are analysis inputs.
    /// </summary>
    public static readonly string[] Properties = new[]
    {
        nameof(Content.Headline),
        nameof(Content.Body),
        nameof(Content.Byline),
        nameof(Content.Summary),
        nameof(Content.IsApproved),
        nameof(Content.SourceId),
        nameof(Content.MediaTypeId),
        nameof(Content.PublishedOn),
    };

    /// <summary>
    /// Compute the fingerprint. The summary is an input only when a person owns it; a generated
    /// summary is never an input to its own analysis. Approval is an input only for audio/video,
    /// whose transcripts await approval.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="isSummaryHumanOwned"></param>
    /// <returns></returns>
    public static string ComputeHash(Content content, bool isSummaryHumanOwned)
    {
        var builder = new StringBuilder();
        builder.Append(Version).Append('\n')
            .Append(content.Headline).Append('\n')
            .Append(content.Body).Append('\n')
            .Append(content.Byline).Append('\n')
            .Append(isSummaryHumanOwned ? content.Summary : "").Append('\n')
            .Append(content.ContentType == ContentType.AudioVideo && content.IsApproved ? "approved" : "").Append('\n')
            .Append(content.SourceId).Append('\n')
            .Append(content.MediaTypeId).Append('\n')
            .Append(ToStoredPrecision(content.PublishedOn)?.ToString("o"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    /// <summary>
    /// The date as PostgreSQL stores it (microseconds; Npgsql truncates the rest), so the hash of
    /// content that has not been saved yet matches the hash of the same content read back.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static DateTime? ToStoredPrecision(DateTime? value)
    {
        if (value == null) return null;
        var utc = value.Value.ToUniversalTime();
        return new DateTime(utc.Ticks - utc.Ticks % 10, DateTimeKind.Utc);
    }

    /// <summary>
    /// A short, stable key for a quote statement, for de-duplication and cleared markers.
    /// </summary>
    /// <param name="statement"></param>
    /// <returns></returns>
    public static string QuoteKey(string statement)
    {
        var normalized = String.Join(' ', statement.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Trim('"', '“', '”', '\'', ' ', '.', ',');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)))[..32].ToLowerInvariant();
    }
}
