using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Core.Exceptions;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.API.Helpers;

/// <summary>A content item present in only one of the stores being compared.</summary>
public record OrphanedContentItem(long Id, string Headline, string Status, string Resolution = "");
/// <summary>Compare the configured search indexes with PostgreSQL and repair individual items.</summary>
public class OrphanedContentService(TNOContext context, IContentService contents, ITNOElasticClient elastic,
    IOptions<ElasticOptions> elasticOptions, IOptions<JsonSerializerOptions> jsonOptions,
    ILogger<OrphanedContentService> logger)
{
    private readonly ElasticOptions _options = elasticOptions.Value;
    private readonly JsonSerializerOptions _json = jsonOptions.Value;

    internal string Index(string index) => index switch
    {
        "content" => _options.ContentIndex,
        "published" => _options.PublishedIndex,
        _ => throw new BadRequestException("Choose the content or published index."),
    };

    internal static void ValidateSide(string side)
    {
        if (side != "postgres" && side != "elasticsearch")
            throw new BadRequestException("Choose postgres or elasticsearch as the store to scan.");
    }

    internal IQueryable<Entities.Content> Eligible(string index)
    {
        _ = Index(index);
        var query = context.Contents.AsNoTracking().Where(c => !c.IsPrivate);
        return index == "published"
            ? query.Where(c => c.Status == ContentStatus.Publish || c.Status == ContentStatus.Published)
            : query;
    }

    private async Task<JsonElement[]> SearchAsync(string index, object query, int size, CancellationToken cancellationToken, bool fullDocument = false)
    {
        var body = JsonSerializer.Serialize(new
        {
            size, query, sort = new[] { new { id = "asc" } }, version = true, track_total_hits = false,
            _source = fullDocument ? (object)true : new[] { "id", "headline", "status" },
        });
        var response = await elastic.LowLevel.SearchAsync<StringResponse>(Index(index), PostData.String(body), ctx: cancellationToken);
        if (!response.Success) throw new InvalidOperationException($"Unable to read the {index} index. No changes were made.", response.OriginalException);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        if ((root.TryGetProperty("timed_out", out var timedOut) && timedOut.GetBoolean()) ||
            (root.TryGetProperty("_shards", out var shards) && shards.GetProperty("failed").GetInt32() != 0))
            throw new InvalidOperationException("Elasticsearch returned incomplete results. Retry the scan.");
        return root.GetProperty("hits").GetProperty("hits").EnumerateArray().Select(h => h.Clone()).ToArray();
    }

    private static long Id(JsonElement hit)
    {
        var id = hit.GetProperty("_source").GetProperty("id").GetInt64();
        if (id <= 0 || hit.GetProperty("_id").GetString() != id.ToString(System.Globalization.CultureInfo.InvariantCulture))
            throw new InvalidOperationException("An Elasticsearch document has an invalid content ID. Repair requires a matching document and content ID.");
        return id;
    }

    private async Task<JsonElement?> FindDocumentAsync(long id, string index, CancellationToken cancellationToken)
    {
        var hits = await SearchAsync(index, new { ids = new { values = new[] { id.ToString(System.Globalization.CultureInfo.InvariantCulture) } } }, 1, cancellationToken, fullDocument: true);
        if (hits.Length == 0) return null;
        if (Id(hits[0]) != id) throw new InvalidOperationException("Elasticsearch returned a different content ID.");
        return hits[0];
    }

    private async Task RequireOrphanAsync(long id, string side, string index, CancellationToken cancellationToken)
    {
        ValidateSide(side);
        if (id <= 0) throw new BadRequestException("A positive content ID is required.");
        var exists = await context.Contents.AsNoTracking().AnyAsync(c => c.Id == id, cancellationToken);
        var document = await FindDocumentAsync(id, index, cancellationToken);
        if (side == "postgres" && (!exists || document != null || !await Eligible(index).AnyAsync(c => c.Id == id, cancellationToken)) ||
            side == "elasticsearch" && (exists || document == null))
            throw new BadRequestException("This item is no longer orphaned in the selected store. Scan again before making changes.");
    }

    /// <summary>Queue the normal indexing workflow after rechecking a database-only item.</summary>
    public async Task IndexAsync(long id, string index, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await LockAsync(id, cancellationToken);
        await RequireOrphanAsync(id, "postgres", index, cancellationToken);
        context.RequestIndex(id);
        context.SaveChanges();
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Administrator queued orphaned content {ContentId} for indexing.", id);
    }

    /// <summary>Restore the original ID from the full document when available, without recreating referenced lookup records.</summary>
    public async Task RestoreAsync(long id, string index, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await LockAsync(id, cancellationToken);
        await RequireOrphanAsync(id, "elasticsearch", index, cancellationToken);
        var full = await FindDocumentAsync(id, "content", cancellationToken);
        var published = await FindDocumentAsync(id, "published", cancellationToken);
        var hit = full ?? published ?? throw new BadRequestException("The Elasticsearch document no longer exists.");
        var model = hit.GetProperty("_source").Deserialize<ContentModel>(_json) ?? throw new BadRequestException("The document cannot be restored.");
        await ValidateReferencesAsync(model, cancellationToken);
        // The search snapshot is not a backup of report membership, deliveries, or analysis history.
        // Do not recreate delivery records or infer a new series from its display name.
        model.Notifications = [];
        model.OtherSeries = null;
        var entity = (Entities.Content)model;
        entity.ExternalUid = model.ExternalUid;
        // Child surrogate IDs need fresh sequence values; foreign references retain their original IDs.
        foreach (var file in entity.FileReferences) file.Id = 0;
        foreach (var time in entity.TimeTrackings) time.Id = 0;
        foreach (var quote in entity.Quotes) quote.Id = 0;
        var validation = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), validation, true))
            throw new BadRequestException(String.Join(" ", validation.Select(v => v.ErrorMessage)));

        // Serialize explicit-ID restoration with ordinary inserts while advancing the sequence.
        await context.Database.ExecuteSqlRawAsync("LOCK TABLE public.content IN SHARE ROW EXCLUSIVE MODE", cancellationToken);
        if (await context.Contents.AnyAsync(c => c.Id == id, cancellationToken))
            throw new BadRequestException("Content was restored by another request. Scan again.");
        context.Contents.Add(entity);
        context.SaveChanges();
        var revision = Math.Max(full?.GetProperty("_version").GetInt64() ?? 0, published?.GetProperty("_version").GetInt64() ?? 0);
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE public.content SET projection_revision = {revision} WHERE id = {id}", cancellationToken);
        await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('public.content', 'id'), GREATEST((SELECT MAX(id) FROM public.content), nextval(pg_get_serial_sequence('public.content', 'id'))), true)", cancellationToken);
        if (!entity.IsPrivate) context.RequestIndex(id);
        context.SaveChanges();
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Administrator restored orphaned content {ContentId} to PostgreSQL from {Index}.", id, full != null ? "content" : "published");
    }

    /// <summary>Delete an item only while it remains orphaned; never accept a client-supplied document.</summary>
    public async Task DeleteAsync(long id, string side, string index, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await LockAsync(id, cancellationToken);
        await RequireOrphanAsync(id, side, index, cancellationToken);
        if (side == "postgres")
        {
            var entity = contents.FindById(id) ?? throw new BadRequestException("Content no longer exists.");
            context.RequestIndex(id, IndexRequestAction.Delete);
            context.Contents.Remove(entity);
            context.SaveChanges();
        }
        else
        {
            foreach (var name in new[] { _options.PublishedIndex, _options.ContentIndex, _options.EvidenceIndex }.Where(n => !String.IsNullOrWhiteSpace(n)).Distinct())
            {
                var response = await elastic.DeleteAsync<ContentModel>(id, d => d.Index(name).Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);
                if (!response.IsValid && response.ApiCall.HttpStatusCode != 404)
                    throw new InvalidOperationException($"Failed to delete content {id} from index '{name}'. Retry the operation.", response.OriginalException);
            }
        }
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Administrator deleted orphaned content {ContentId} from {Side}.", id, side);
    }

    private Task<int> LockAsync(long id, CancellationToken cancellationToken)
        => context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({id})", cancellationToken);

    private async Task ValidateReferencesAsync(ContentModel model, CancellationToken cancellationToken)
    {
        var missing = new List<string>();
        if (!await context.Licenses.AnyAsync(x => x.Id == model.LicenseId, cancellationToken)) missing.Add($"license {model.LicenseId}");
        if (!await context.MediaTypes.AnyAsync(x => x.Id == model.MediaTypeId, cancellationToken)) missing.Add($"media type {model.MediaTypeId}");
        if (model.SourceId.HasValue && !await context.Sources.AnyAsync(x => x.Id == model.SourceId, cancellationToken)) missing.Add($"source {model.SourceId}");
        if (model.SeriesId.HasValue && !await context.Series.AnyAsync(x => x.Id == model.SeriesId, cancellationToken)) missing.Add($"series {model.SeriesId}");
        if (model.ContributorId.HasValue && !await context.Contributors.AnyAsync(x => x.Id == model.ContributorId, cancellationToken)) model.ContributorId = null;
        if (model.OwnerId.HasValue && !await context.Users.AnyAsync(x => x.Id == model.OwnerId, cancellationToken)) missing.Add($"owner {model.OwnerId}");
        var users = model.TimeTrackings.Select(t => t.UserId).Distinct().ToArray();
        missing.AddRange(users.Except(await context.Users.Where(x => users.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(cancellationToken)).Select(id => $"time tracking user {id}"));
        var topics = model.Topics.Select(t => t.Id).ToArray();
        var tags = model.Tags.Select(t => t.Id).ToArray();
        var actions = model.Actions.Select(t => t.Id).ToArray();
        var tones = model.TonePools.Select(t => t.Id).ToArray();
        missing.AddRange(topics.Except(await context.Topics.Where(x => topics.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(cancellationToken)).Select(id => $"topic {id}"));
        missing.AddRange(tags.Except(await context.Tags.Where(x => tags.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(cancellationToken)).Select(id => $"tag {id}"));
        missing.AddRange(actions.Except(await context.Actions.Where(x => actions.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(cancellationToken)).Select(id => $"action {id}"));
        missing.AddRange(tones.Except(await context.TonePools.Where(x => tones.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(cancellationToken)).Select(id => $"tone pool {id}"));
        if (missing.Count > 0) throw new BadRequestException($"Cannot restore content: referenced records are missing ({String.Join(", ", missing)}). Restore those records first or delete the orphan.");
    }
}
