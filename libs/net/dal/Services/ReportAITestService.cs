using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ReportAITestService class, stores the short-lived test runs of AI report sections. The rows
/// are the hand-off between the API deployment the browser uses and the one the reporting service
/// posts to, so nothing about a run may live in process memory.
/// </summary>
public class ReportAITestService : BaseService, IReportAITestService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAITestService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public ReportAITestService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ReportAITestService> logger)
        : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public ReportAITest? FindById(Guid id)
    {
        return this.Context.ReportAITests.AsNoTracking().FirstOrDefault(t => t.Id == id);
    }

    /// <inheritdoc/>
    public ReportAITest? FindPending(int requestorId, int sectionId, DateTime since)
    {
        return this.Context.ReportAITests.AsNoTracking()
            .Where(t => t.RequestorId == requestorId && t.ReportSectionId == sectionId && t.Status == ReportAITestStatus.Pending && t.CreatedOn >= since)
            .OrderByDescending(t => t.CreatedOn)
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    public ReportAITest Start(ReportAITest test)
    {
        var replaced = this.Context.ReportAITests
            .Where(t => t.RequestorId == test.RequestorId && t.ReportSectionId == test.ReportSectionId && t.Id != test.Id)
            .ToArray();
        this.Context.ReportAITests.RemoveRange(replaced);
        test.Status = ReportAITestStatus.Pending;
        this.Context.ReportAITests.Add(test);
        this.Context.CommitTransaction();
        return test;
    }

    /// <inheritdoc/>
    public ReportAITest Complete(ReportAITest test)
    {
        var stored = this.Context.ReportAITests.FirstOrDefault(t => t.Id == test.Id);
        if (stored == null)
        {
            stored = test;
            this.Context.ReportAITests.Add(stored);
        }
        else
        {
            stored.Output = test.Output;
            stored.Error = test.Error;
            stored.RequestCount = test.RequestCount;
            stored.PromptTokens = test.PromptTokens;
            stored.CompletionTokens = test.CompletionTokens;
            stored.DurationMs = test.DurationMs;
            stored.StoryCount = test.StoryCount;
            stored.ReductionDepth = test.ReductionDepth;
            stored.Transcript = test.Transcript;
        }
        // A posted outcome is never left pending, or the browser would wait for it forever.
        stored.Status = test.Status == ReportAITestStatus.Pending ? ReportAITestStatus.Failed : test.Status;
        stored.CompletedOn = test.CompletedOn ?? DateTime.UtcNow;
        this.Context.CommitTransaction();
        return stored;
    }

    /// <inheritdoc/>
    public bool Delete(Guid id)
    {
        var stored = this.Context.ReportAITests.FirstOrDefault(t => t.Id == id);
        if (stored == null) return false;
        this.Context.ReportAITests.Remove(stored);
        this.Context.CommitTransaction();
        return true;
    }

    /// <inheritdoc/>
    public int Purge(DateTime olderThan)
    {
        // Loaded rather than ExecuteDelete so the in-memory provider used by the tests can run it; the table holds a handful of rows.
        var stale = this.Context.ReportAITests.Where(t => t.CreatedOn < olderThan).ToArray();
        if (stale.Length == 0) return 0;
        this.Context.ReportAITests.RemoveRange(stale);
        this.Context.CommitTransaction();
        return stale.Length;
    }
    #endregion
}
