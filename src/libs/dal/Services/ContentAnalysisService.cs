using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Core.Exceptions;
using TNO.DAL.Analysis;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// ContentAnalysisService class, the Content-Analysis inputs, the acceptance of results, and the
/// record of each content item's recent analysis runs (the 'analysis' key of the content's
/// metadata). The work itself arrives through Kafka.
/// </summary>
public class ContentAnalysisService : BaseService, IContentAnalysisService
{
    #region Variables
    /// <summary>The submission was stored and applied.</summary>
    public const string Accepted = "Accepted";
    /// <summary>The same input was already analyzed; the stored result is returned.</summary>
    public const string Duplicate = "Duplicate";
    /// <summary>The submission is out of date and was rejected without populating anything.</summary>
    public const string Stale = "Stale";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// How the analysis metadata is stored: camel case with enum names, so the failed-content index
    /// can match 'Failed'.
    /// </summary>
    private static readonly JsonSerializerOptions _metadataOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// The content's analysis metadata as text.
    /// </summary>
    private class AnalysisRow
    {
        public long Id { get; set; }
        public string Headline { get; set; } = "";
        public string? Analysis { get; set; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public ContentAnalysisService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ContentAnalysisService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// The runtime settings.
    /// </summary>
    /// <returns></returns>
    public ContentAnalysisSettings GetSettings() => ContentAnalysisSettings.Read(this.Context);

    /// <summary>
    /// The content's current analysis input, or null when the content does not exist.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public AnalysisInputModel? GetInput(long contentId)
    {
        var content = this.Context.Contents.AsNoTracking()
            .Include(c => c.Source)
            .Include(c => c.MediaType)
            .Include(c => c.Series)
            .FirstOrDefault(c => c.Id == contentId);
        if (content == null) return null;

        var settings = GetSettings();
        var ineligible = settings.GetIneligibleReason(content);
        var isSummaryHumanOwned = this.Context.IsSummaryHumanOwned(content.Id);
        return new AnalysisInputModel()
        {
            ContentId = content.Id,
            InputHash = AnalysisInput.ComputeHash(content, isSummaryHumanOwned),
            AnalysisInputHash = this.Context.ContentAnalyses.AsNoTracking().Where(a => a.ContentId == content.Id && a.IsCurrent).Select(a => a.InputHash).FirstOrDefault(),
            IsEligible = ineligible == null,
            IneligibleReason = ineligible,
            ContentType = content.ContentType,
            Headline = content.Headline,
            Byline = content.Byline,
            Body = content.Body,
            Summary = isSummaryHumanOwned ? content.Summary : "",
            Source = content.Source?.Name ?? content.OtherSource,
            SourceId = content.SourceId,
            MediaType = content.MediaType?.Name ?? "",
            MediaTypeId = content.MediaTypeId,
            Series = content.Series?.Name ?? "",
            PublishedOn = content.PublishedOn,
            IsApproved = content.IsApproved,
            LLMId = settings.LLMId,
        };
    }

    /// <summary>
    /// Accept an analysis when its input is still the content's current input, populate the empty
    /// fields of the processes run, and record the request's run.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public AnalysisSubmitResultModel Submit(AnalysisResultModel result)
    {
        var duplicate = this.Context.ContentAnalyses.AsNoTracking().FirstOrDefault(a => a.ContentId == result.ContentId && a.InputHash == result.InputHash);
        if (duplicate != null)
        {
            RecordRun(result.ContentId, result.Request.ToRun(AnalysisRunStatus.Completed, analysisId: duplicate.Id));
            return new AnalysisSubmitResultModel() { Status = Duplicate, AnalysisId = duplicate.Id };
        }

        var content = this.Context.Contents
            .Include(c => c.TagsManyToMany)
            .Include(c => c.TopicsManyToMany)
            .Include(c => c.Quotes)
            .FirstOrDefault(c => c.Id == result.ContentId);
        if (content == null) return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The content no longer exists." };

        var settings = GetSettings();
        var ineligible = settings.GetIneligibleReason(content);
        if (ineligible != null) return new AnalysisSubmitResultModel() { Status = Stale, Reason = ineligible };
        // A newer request for the changed input follows this one.
        if (AnalysisInput.ComputeHash(content, this.Context.IsSummaryHumanOwned(content.Id)) != result.InputHash)
            return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The content changed after it was analyzed." };

        using var transaction = this.Context.Database.BeginTransaction();
        this.Context.ChangeOwner = FieldOwner.Analysis;
        try
        {
            foreach (var previous in this.Context.ContentAnalyses.Where(a => a.ContentId == content.Id && a.IsCurrent))
                previous.IsCurrent = false;

            var registry = ResolveRegistry(result.PrimaryTopic);
            var analysis = ToEntity(result, content.Id, registry);
            this.Context.ContentAnalyses.Add(analysis);
            this.Context.SaveChanges();

            // Only the fields of the processes the worker ran are applied.
            var populator = new AnalysisPopulator(this.Context, settings, content, analysis);
            populator.Populate(result);
            var populated = populator.PopulatedFields.ToArray();
            // Populating any field changes the content's version, so an open editor form merges it.
            if (populated.Length > 0) this.Context.Entry(content).Property(c => c.UpdatedOn).IsModified = true;

            // The analysis is part of the indexed document, so the content is always re-indexed.
            this.Context.RequestIndex(content, TNOContext.GetIndexAction(content.Status), TNOContext.IndexReasonAnalysis);
            this.Context.SaveChanges();
            RecordRun(content.Id, result.Request.ToRun(AnalysisRunStatus.Completed, analysisId: analysis.Id));
            transaction.Commit();

            return new AnalysisSubmitResultModel()
            {
                Status = Accepted,
                AnalysisId = analysis.Id,
                PopulatedFields = populated,
                ContentChanged = populated.Length > 0,
                ContentStatus = content.Status,
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            this.Context.ChangeOwner = null;
        }
    }

    /// <summary>
    /// The registry entry for a topic label: matched by its deterministic key or an alias, otherwise
    /// added, linked to the staff topic of the same name when there is one.
    /// </summary>
    private AnalysisTopic? ResolveRegistry(string? label)
    {
        var key = AnalysisPopulator.Normalize(label);
        if (key.Length == 0) return null;
        var entry = this.Context.AnalysisTopics.FirstOrDefault(t => t.Key == key || t.Aliases.Contains(key));
        if (entry != null) return entry;

        var staffTopic = this.Context.Topics.AsNoTracking().Where(t => !t.IsSystem).Select(t => new { t.Id, t.Name }).ToArray()
            .FirstOrDefault(t => AnalysisPopulator.Normalize(t.Name) == key);
        entry = new AnalysisTopic(label!.Trim(), key) { TopicId = staffTopic?.Id };
        this.Context.AnalysisTopics.Add(entry);
        this.Context.SaveChanges();
        return entry;
    }

    private static ContentAnalysis ToEntity(AnalysisResultModel result, long contentId, AnalysisTopic? registry)
    {
        JsonDocument Json(object value) => JsonSerializer.SerializeToDocument(value, _jsonOptions);
        return new ContentAnalysis(contentId, result.InputHash)
        {
            IsCurrent = true,
            IsMetadataOnly = result.IsMetadataOnly,
            NormalizationVersion = result.NormalizationVersion,
            SchemaVersion = result.SchemaVersion,
            PromptVersion = result.PromptVersion,
            LLMId = result.LLMId,
            Model = result.Model,
            Summary = result.Summary,
            KeyFacts = Json(result.KeyFacts),
            Entities = Json(result.Entities),
            Places = Json(result.Places),
            Topics = Json(result.Topics),
            PrimaryTopic = result.PrimaryTopic,
            AnalysisTopicId = registry?.Id,
            SuggestedTags = Json(result.SuggestedTags),
            SuggestedContributor = result.SuggestedContributor,
            Events = Json(result.Events),
            Quotes = Json(result.Quotes),
            // The processes the worker ran are recorded with the analysis.
            Validation = Json(new Dictionary<string, object?>(result.Validation) { ["processes"] = result.Processes.ToString() }),
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
        };
    }


    /// <summary>
    /// Record the outcome of an analysis request in the content's metadata. The content row is locked
    /// while the runs are merged, so concurrent outcomes (lifecycle, retry, backfill) are applied one
    /// at a time; the write is raw SQL, so the content's concurrency version does not change.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="run"></param>
    /// <returns></returns>
    public AnalysisMetadata? RecordRun(long contentId, AnalysisRun run)
    {
        var ownsTransaction = this.Context.Database.CurrentTransaction == null;
        var transaction = ownsTransaction ? this.Context.Database.BeginTransaction() : null;
        try
        {
            var row = this.Context.Database.SqlQueryRaw<AnalysisRow>(
                    @"SELECT id AS ""Id"", headline AS ""Headline"", (metadata -> 'analysis')::text AS ""Analysis"" FROM public.content WHERE id = {0} FOR UPDATE", contentId)
                .AsEnumerable()
                .FirstOrDefault();
            if (row == null) return null;

            var metadata = Deserialize(row.Analysis).Record(run);
            this.Context.Database.ExecuteSqlRaw(
                "UPDATE public.content SET metadata = jsonb_set(metadata, ARRAY['analysis'], {1}::jsonb) WHERE id = {0}",
                contentId, JsonSerializer.Serialize(metadata, _metadataOptions));
            transaction?.Commit();
            return metadata;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    /// <summary>
    /// The content's recent analysis runs.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public AnalysisMetadata FindRuns(long contentId)
    {
        var analysis = this.Context.Database.SqlQueryRaw<string>(
                @"SELECT (metadata -> 'analysis')::text AS ""Value"" FROM public.content WHERE id = {0}", contentId)
            .AsEnumerable()
            .FirstOrDefault();
        return Deserialize(analysis);
    }

    /// <summary>
    /// Content whose newest analysis request failed, most recent first.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    public IEnumerable<(long ContentId, string Headline, AnalysisRun Run)> FindFailures(int qty = 100)
    {
        // The predicate matches the partial index 'IX_content_analysis_failed'.
        return this.Context.Database.SqlQueryRaw<AnalysisRow>(@"
SELECT id AS ""Id"", headline AS ""Headline"", (metadata -> 'analysis')::text AS ""Analysis""
FROM public.content
WHERE metadata -> 'analysis' ->> 'status' = 'Failed'
ORDER BY metadata -> 'analysis' -> 'runs' -> 0 ->> 'finishedOn' DESC
LIMIT {0}", Math.Clamp(qty, 1, 1000))
            .AsEnumerable()
            .Select(r => (Row: r, Run: Deserialize(r.Analysis).Runs.FirstOrDefault()))
            .Where(r => r.Run != null)
            .Select(r => (r.Row.Id, r.Row.Headline, r.Run!))
            .ToArray();
    }

    /// <summary>
    /// The number of content items whose newest analysis request failed, optionally only those sent
    /// by the specified backfill work order.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <returns></returns>
    public int CountFailures(long? workOrderId = null)
    {
        return this.Context.Database.SqlQueryRaw<int>(@"
SELECT COUNT(*)::int AS ""Value""
FROM public.content
WHERE metadata -> 'analysis' ->> 'status' = 'Failed'
    AND ({0}::bigint IS NULL OR (metadata -> 'analysis' -> 'runs' -> 0 ->> 'workOrderId')::bigint = {0}::bigint)",
                new Npgsql.NpgsqlParameter() { Value = (object?)workOrderId ?? DBNull.Value, NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint })
            .AsEnumerable()
            .First();
    }

    /// <summary>
    /// Request analysis of the content's current input, even when its analysis is current; the API
    /// sends the request once the action completes.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException">The content does not exist.</exception>
    public SavedAnalysisRequest RequestAnalysis(long contentId, AnalysisRequestReason reason)
    {
        var content = this.Context.Contents.AsNoTracking().FirstOrDefault(c => c.Id == contentId) ?? throw new NoContentException("Content does not exist");
        var request = SavedAnalysisRequest.Create(contentId, AnalysisInput.ComputeHash(content, this.Context.IsSummaryHumanOwned(contentId)), reason, true);
        this.Context.RequestAnalysis(request);
        return request;
    }

    /// <summary>
    /// The stored analysis metadata.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private static AnalysisMetadata Deserialize(string? json)
    {
        if (String.IsNullOrWhiteSpace(json)) return new AnalysisMetadata();
        return JsonSerializer.Deserialize<AnalysisMetadata>(json, _metadataOptions) ?? new AnalysisMetadata();
    }

    /// <summary>
    /// The content's current analysis.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public ContentAnalysis? FindCurrent(long contentId)
        => this.Context.ContentAnalyses.AsNoTracking().Include(a => a.AnalysisTopic).FirstOrDefault(a => a.ContentId == contentId && a.IsCurrent);

    /// <summary>
    /// The current analyses of the specified content.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <returns></returns>
    public IDictionary<long, ContentAnalysis> FindCurrent(IEnumerable<long> contentIds)
    {
        var ids = contentIds.Distinct().ToArray();
        var result = new Dictionary<long, ContentAnalysis>();
        foreach (var chunk in ids.Chunk(500))
        {
            foreach (var analysis in this.Context.ContentAnalyses.AsNoTracking().Include(a => a.AnalysisTopic).ThenInclude(t => t!.Topic)
                .Where(a => chunk.Contains(a.ContentId) && a.IsCurrent))
                result[analysis.ContentId] = analysis;
        }
        return result;
    }

    /// <summary>
    /// The ownership records of the content's editorial values.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<ContentFieldOwnership> FindOwnership(long contentId)
        => this.Context.ContentFieldOwnerships.AsNoTracking().Where(o => o.ContentId == contentId).ToArray();

    #endregion
}
