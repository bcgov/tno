using TNO.API.Areas.Services.Models.History;

namespace TNO.DAL.Services;

/// <summary>
/// IHistoryRetentionService interface, purges report and notification history older than the
/// configured retention.
/// </summary>
public interface IHistoryRetentionService : IBaseService
{
    /// <summary>
    /// The default report retention in days, used when the setting does not exist.
    /// </summary>
    const int DefaultReportRetentionDays = 90;

    /// <summary>
    /// The default notification retention in days, used when the setting does not exist.
    /// </summary>
    const int DefaultNotificationRetentionDays = 30;

    /// <summary>
    /// The configured report retention in days ('ReportRetentionDays' setting).
    /// </summary>
    /// <returns></returns>
    int GetReportRetentionDays();

    /// <summary>
    /// The configured notification retention in days ('NotificationRetentionDays' setting).
    /// </summary>
    /// <returns></returns>
    int GetNotificationRetentionDays();

    /// <summary>
    /// Delete report instances and evening overview instances older than 'retentionDays',
    /// always keeping the instances report logic depends on.
    /// </summary>
    /// <param name="retentionDays">Zero or less disables the purge.</param>
    /// <param name="dryRun">Count what would be deleted without deleting it.</param>
    /// <param name="batchSize">Rows deleted per statement.</param>
    /// <returns></returns>
    HistoryPurgeModel PurgeReports(int retentionDays, bool dryRun = false, int batchSize = 500);

    /// <summary>
    /// Delete notification instances older than each notification's cutoff, which is the longest of
    /// 'retentionDays', its filter date window, and 'publishedBeforeOffsetDays'.
    /// </summary>
    /// <param name="retentionDays">Zero or less disables the purge.</param>
    /// <param name="publishedBeforeOffsetDays">The notification service's IgnoreContentPublishedBeforeOffset.</param>
    /// <param name="dryRun">Count what would be deleted without deleting it.</param>
    /// <param name="batchSize">Rows deleted per statement.</param>
    /// <returns></returns>
    HistoryPurgeModel PurgeNotifications(int retentionDays, int? publishedBeforeOffsetDays, bool dryRun = false, int batchSize = 500);
}
