using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ReportAIResultService class, stores generated AI section results so an unchanged manifest is
/// generated once, across the API and the reporting service.
/// </summary>
public class ReportAIResultService : BaseService, IReportAIResultService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIResultService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public ReportAIResultService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ReportAIResultService> logger)
        : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find the result for the specified manifest hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public ReportAIResult? FindByHash(string hash)
    {
        return this.Context.ReportAIResults.AsNoTracking().FirstOrDefault(r => r.Hash == hash);
    }

    /// <summary>
    /// Atomically claim the right to generate a result.
    /// </summary>
    /// <param name="claim"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public ReportAIResult? TryClaim(ReportAIResultClaimModel claim, string username)
    {
        // One statement, so two generators can never both hold the claim: insert when missing, or
        // take over a failed result or a lapsed claim. A completed or actively claimed row is left alone.
        const string sql = @"
INSERT INTO public.report_ai_result (hash, report_id, report_instance_id, report_section_id, status, manifest, output, pipeline_version, claim_expires_on,
    request_count, prompt_tokens, completion_tokens, duration_ms, story_count, reduction_depth, created_by, updated_by)
VALUES (@hash, @reportId, @reportInstanceId, @reportSectionId, 0, @manifest::jsonb, '', @pipelineVersion, @expires,
    0, 0, 0, 0, 0, 0, @username, @username)
ON CONFLICT (hash) DO UPDATE
SET status = 0,
    error = NULL,
    claim_expires_on = EXCLUDED.claim_expires_on,
    report_instance_id = COALESCE(public.report_ai_result.report_instance_id, EXCLUDED.report_instance_id),
    updated_by = EXCLUDED.updated_by,
    updated_on = CURRENT_TIMESTAMP,
    version = public.report_ai_result.version + 1
WHERE public.report_ai_result.status = 2
    OR (public.report_ai_result.status = 0 AND public.report_ai_result.claim_expires_on < CURRENT_TIMESTAMP)
RETURNING id AS ""Value""";

        var ids = this.Context.Database.SqlQueryRaw<long>(sql,
                new Npgsql.NpgsqlParameter("hash", claim.Hash),
                new Npgsql.NpgsqlParameter("reportId", claim.ReportId),
                new Npgsql.NpgsqlParameter("reportInstanceId", (object?)claim.ReportInstanceId ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("reportSectionId", claim.ReportSectionId),
                new Npgsql.NpgsqlParameter("manifest", claim.Manifest.RootElement.GetRawText()),
                new Npgsql.NpgsqlParameter("pipelineVersion", claim.PipelineVersion),
                new Npgsql.NpgsqlParameter("expires", DateTime.UtcNow.AddSeconds(Math.Max(30, claim.LeaseSeconds))),
                new Npgsql.NpgsqlParameter("username", username))
            .ToArray();
        if (ids.Length == 0) return null;
        return this.Context.ReportAIResults.AsNoTracking().First(r => r.Id == ids[0]);
    }

    /// <summary>
    /// Record the outcome of a claimed result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completion"></param>
    /// <returns></returns>
    public ReportAIResult Complete(long id, ReportAIResultCompletionModel completion)
    {
        // One statement, like the claim. It is saved during report generation, when the request's
        // context may track entities attached only to build the report; a SaveChanges would detect
        // and try to save them too.
        var status = completion.IsSuccess ? ReportAIResultStatus.Completed : ReportAIResultStatus.Failed;
        var output = completion.IsSuccess ? completion.Output : "";
        var username = this.Principal.GetUsername() ?? "";
        var affected = this.Context.ReportAIResults
            .Where(r => r.Id == id)
            .ExecuteUpdate(setters => setters
                .SetProperty(r => r.Status, status)
                .SetProperty(r => r.Output, output)
                .SetProperty(r => r.Error, completion.Error)
                .SetProperty(r => r.RequestCount, completion.RequestCount)
                .SetProperty(r => r.PromptTokens, completion.PromptTokens)
                .SetProperty(r => r.CompletionTokens, completion.CompletionTokens)
                .SetProperty(r => r.DurationMs, completion.DurationMs)
                .SetProperty(r => r.StoryCount, completion.StoryCount)
                .SetProperty(r => r.ReductionDepth, completion.ReductionDepth)
                .SetProperty(r => r.ClaimExpiresOn, (DateTime?)null)
                .SetProperty(r => r.UpdatedBy, username)
                .SetProperty(r => r.UpdatedOn, DateTime.UtcNow)
                .SetProperty(r => r.Version, r => r.Version + 1));
        if (affected == 0) throw new NoContentException("AI result does not exist");
        return this.Context.ReportAIResults.AsNoTracking().First(r => r.Id == id);
    }
    #endregion
}
