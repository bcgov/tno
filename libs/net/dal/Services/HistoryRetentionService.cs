using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Settings;
using TNO.API.Areas.Services.Models.History;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// HistoryRetentionService class, purges report and notification history older than the
/// configured retention, in batches.
/// </summary>
public class HistoryRetentionService : BaseService, IHistoryRetentionService
{
    #region Variables
    /// <summary>
    /// Sent instances kept when a report removes duplicate titles over three days; matches the
    /// history ReportService.FindContentWithElasticsearchAsync reads.
    /// </summary>
    public const int DuplicateTitleHistoryQty = 10;

    /// <summary>
    /// Sent instances kept for every other report; matches the history
    /// ReportService.FindContentWithElasticsearchAsync reads.
    /// </summary>
    public const int DefaultHistoryQty = 2;

    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HistoryRetentionService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public HistoryRetentionService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<HistoryRetentionService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// The configured report retention in days ('ReportRetentionDays' setting).
    /// </summary>
    /// <returns></returns>
    public int GetReportRetentionDays()
    {
        return GetSettingDays(AdminConfigurableSettingNames.ReportRetentionDays, IHistoryRetentionService.DefaultReportRetentionDays);
    }

    /// <summary>
    /// The configured notification retention in days ('NotificationRetentionDays' setting).
    /// </summary>
    /// <returns></returns>
    public int GetNotificationRetentionDays()
    {
        return GetSettingDays(AdminConfigurableSettingNames.NotificationRetentionDays, IHistoryRetentionService.DefaultNotificationRetentionDays);
    }

    /// <summary>
    /// Read a whole number of days from the setting table. A missing setting uses the default; a
    /// disabled or unparseable setting disables the purge, so a typo never deletes history.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultDays"></param>
    /// <returns></returns>
    private int GetSettingDays(AdminConfigurableSettingNames name, int defaultDays)
    {
        var key = name.ToString();
        var setting = this.Context.Settings.AsNoTracking().FirstOrDefault(s => s.Name == key);
        if (setting == null) return defaultDays;
        if (!setting.IsEnabled) return 0;
        return int.TryParse(setting.Value, out var days) && days > 0 ? days : 0;
    }

    /// <summary>
    /// The number of most recent sent instances to keep for a report, beside its latest instance.
    /// This is the history the report logic reads: previous-instance AI input, duplicate-title
    /// removal, and the prior instances used by OnlyNewContent and ExcludeHistorical.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    public static int GetProtectedSentQty(ReportSettingsModel report, IEnumerable<(ReportSectionType SectionType, ReportSectionSettingsModel Settings)> sections)
    {
        var sectionArray = sections.ToArray();
        var previousReports = sectionArray
            .Where(s => s.SectionType == ReportSectionType.AI)
            .Select(s => s.Settings.IncludePreviousReports ?? 0)
            .DefaultIfEmpty(0)
            .Max();
        var history = report.Content.RemoveDuplicateTitles3Days || sectionArray.Any(s => s.Settings.RemoveDuplicateTitles3Days)
            ? DuplicateTitleHistoryQty
            : DefaultHistoryQty;
        return Math.Max(previousReports, history);
    }

    /// <summary>
    /// Delete report instances and evening overview instances older than 'retentionDays',
    /// always keeping the instances report logic depends on.
    /// Per report and owner the latest instance is kept, and so are the most recent sent instances
    /// before it (see GetProtectedSentQty). Outside that set, sent instances are deleted by
    /// 'sent_on' and unsent instances by 'created_on'. Deleting an instance cascades to its
    /// content links, user instances, and cached AI results; content itself is untouched.
    /// </summary>
    /// <param name="retentionDays"></param>
    /// <param name="dryRun"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public HistoryPurgeModel PurgeReports(int retentionDays, bool dryRun = false, int batchSize = 500)
    {
        var result = new HistoryPurgeModel() { DryRun = dryRun, RetentionDays = Math.Max(0, retentionDays) };
        if (retentionDays <= 0) return result;
        batchSize = Math.Max(1, batchSize);
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        // The protected history differs per report, so compute it here and hand it to the query.
        var reports = this.Context.Reports
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                r.Settings,
                Sections = r.Sections.Select(s => new { s.SectionType, s.Settings }).ToArray(),
            })
            .ToArray();
        var reportIds = reports.Select(r => r.Id).ToArray();
        var keepQty = reports.Select(r => GetProtectedSentQty(
            JsonSerializer.Deserialize<ReportSettingsModel>(r.Settings, _serializerOptions) ?? new ReportSettingsModel(),
            r.Sections.Select(s => (s.SectionType, JsonSerializer.Deserialize<ReportSectionSettingsModel>(s.Settings, _serializerOptions) ?? new ReportSectionSettingsModel())))).ToArray();

        // 'latest_rank' finds the latest instance per report and owner; 'sent_rank' orders the
        // sent instances before it, most recent first, so the first 'keep' of them survive.
        const string sql = @"
WITH k AS (
    SELECT * FROM unnest(@reportIds, @keepQty) AS k(report_id, keep)
), a AS (
    SELECT ri.id, ri.report_id, ri.owner_id, ri.sent_on, ri.created_on, k.keep,
        ROW_NUMBER() OVER (PARTITION BY ri.report_id, ri.owner_id ORDER BY ri.id DESC) AS latest_rank
    FROM public.report_instance ri
    JOIN k ON k.report_id = ri.report_id
), b AS (
    SELECT a.*,
        ROW_NUMBER() OVER (PARTITION BY a.report_id, a.owner_id, (a.latest_rank > 1 AND a.sent_on IS NOT NULL) ORDER BY a.id DESC) AS sent_rank
    FROM a
)
SELECT b.id AS ""Value""
FROM b
WHERE b.latest_rank > 1
    AND NOT (b.sent_on IS NOT NULL AND b.sent_rank <= b.keep)
    AND ((b.sent_on IS NOT NULL AND b.sent_on < @cutoff) OR (b.sent_on IS NULL AND b.created_on < @cutoff))
    AND b.id > @after
ORDER BY b.id
LIMIT @batchSize";

        long after = 0;
        while (true)
        {
            var ids = this.Context.Database.SqlQueryRaw<long>(sql,
                    new Npgsql.NpgsqlParameter("reportIds", reportIds),
                    new Npgsql.NpgsqlParameter("keepQty", keepQty),
                    new Npgsql.NpgsqlParameter("cutoff", cutoff),
                    new Npgsql.NpgsqlParameter("after", after),
                    new Npgsql.NpgsqlParameter("batchSize", batchSize))
                .ToArray();
            if (ids.Length == 0) break;
            after = ids[^1];

            result.Add("report_instance_content", this.Context.ReportInstanceContents.Count(c => ids.Contains(c.InstanceId)));
            result.Add("user_report_instance", this.Context.UserReportInstances.Count(c => ids.Contains(c.InstanceId)));
            result.Add("report_ai_result", this.Context.ReportAIResults.Count(r => r.ReportInstanceId.HasValue && ids.Contains(r.ReportInstanceId.Value)));
            result.Add("report_instance", dryRun
                ? ids.Length
                : this.Context.ReportInstances.Where(ri => ids.Contains(ri.Id)).ExecuteDelete());
        }

        // AI results generated for previews without an instance are purged by age.
        after = 0;
        while (true)
        {
            var ids = this.Context.ReportAIResults
                .AsNoTracking()
                .Where(r => r.ReportInstanceId == null && r.CreatedOn < cutoff && r.Id > after)
                .OrderBy(r => r.Id)
                .Select(r => r.Id)
                .Take(batchSize)
                .ToArray();
            if (ids.Length == 0) break;
            after = ids[^1];
            result.Add("report_ai_result", dryRun
                ? ids.Length
                : this.Context.ReportAIResults.Where(r => ids.Contains(r.Id)).ExecuteDelete());
        }

        // Evening overview instances have no dependent history; delete by publication date.
        after = 0;
        while (true)
        {
            var ids = this.Context.AVOverviewInstances
                .AsNoTracking()
                .Where(i => i.PublishedOn < cutoff && i.Id > after)
                .OrderBy(i => i.Id)
                .Select(i => i.Id)
                .Take(batchSize)
                .ToArray();
            if (ids.Length == 0) break;
            after = ids[^1];

            result.Add("av_overview_section_item", this.Context.AVOverviewSectionItems.Count(i => ids.Contains(i.Section!.InstanceId)));
            result.Add("av_overview_section", this.Context.AVOverviewSections.Count(s => ids.Contains(s.InstanceId)));
            result.Add("user_av_overview_instance", this.Context.UserAVOverviewInstances.Count(u => ids.Contains(u.InstanceId)));
            result.Add("av_overview_instance", dryRun
                ? ids.Length
                : this.Context.AVOverviewInstances.Where(i => ids.Contains(i.Id)).ExecuteDelete());
        }

        this.Logger.LogInformation("Report history purge{dryRun}: retention {days} day(s), {counts}",
            dryRun ? " (dry run)" : "", retentionDays, String.Join(", ", result.Tables.Select(t => $"{t.Key}={t.Value}")));
        return result;
    }

    /// <summary>
    /// Delete notification instances older than each notification's cutoff.
    /// The cutoff keeps the longest of the retention, the filter's date window (a fixed start
    /// date keeps everything since that date), and the notification service's published-before
    /// offset, so the history the 'Never' resend option and filter exclusions rely on survives.
    /// Subscriptions are never purged.
    /// </summary>
    /// <param name="retentionDays"></param>
    /// <param name="publishedBeforeOffsetDays"></param>
    /// <param name="dryRun"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public HistoryPurgeModel PurgeNotifications(int retentionDays, int? publishedBeforeOffsetDays, bool dryRun = false, int batchSize = 500)
    {
        var result = new HistoryPurgeModel() { DryRun = dryRun, RetentionDays = Math.Max(0, retentionDays) };
        if (retentionDays <= 0) return result;
        batchSize = Math.Max(1, batchSize);
        var now = DateTime.UtcNow;

        var notifications = this.Context.Notifications
            .AsNoTracking()
            .Select(n => new { n.Id, n.Settings })
            .ToArray();

        foreach (var notification in notifications)
        {
            var cutoff = GetNotificationCutoff(now, retentionDays, publishedBeforeOffsetDays,
                JsonSerializer.Deserialize<FilterSettingsModel>(notification.Settings, _serializerOptions) ?? new FilterSettingsModel());

            long after = 0;
            while (true)
            {
                var ids = this.Context.NotificationInstances
                    .AsNoTracking()
                    .Where(i => i.NotificationId == notification.Id && i.Id > after
                        && (i.SentOn != null ? i.SentOn < cutoff : i.CreatedOn < cutoff))
                    .OrderBy(i => i.Id)
                    .Select(i => i.Id)
                    .Take(batchSize)
                    .ToArray();
                if (ids.Length == 0) break;
                after = ids[^1];

                result.Add("notification_instance", dryRun
                    ? ids.Length
                    : this.Context.NotificationInstances.Where(i => ids.Contains(i.Id)).ExecuteDelete());
            }
        }

        this.Logger.LogInformation("Notification history purge{dryRun}: retention {days} day(s), {counts}",
            dryRun ? " (dry run)" : "", retentionDays, String.Join(", ", result.Tables.Select(t => $"{t.Key}={t.Value}")));
        return result;
    }

    /// <summary>
    /// The date before which a notification's history can be deleted.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="retentionDays"></param>
    /// <param name="publishedBeforeOffsetDays"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static DateTime GetNotificationCutoff(DateTime now, int retentionDays, int? publishedBeforeOffsetDays, FilterSettingsModel filter)
    {
        var days = new[] { retentionDays, filter.DateOffset ?? 0, publishedBeforeOffsetDays ?? 0 }.Max();
        var cutoff = now.AddDays(-days);
        if (filter.StartDate.HasValue)
        {
            var start = filter.StartDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc)
                : filter.StartDate.Value.ToUniversalTime();
            if (start < cutoff) cutoff = start;
        }
        return cutoff;
    }
    #endregion
}
