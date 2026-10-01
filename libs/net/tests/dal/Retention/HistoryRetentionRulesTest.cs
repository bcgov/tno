using FluentAssertions;
using TNO.API.Models.Settings;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Test.DAL.Retention;

/// <summary>
/// Tests for the rules that decide which history a purge keeps.
/// </summary>
public class HistoryRetentionRulesTest
{
    #region Protected report history
    [Fact]
    public void ProtectedSentQty_DefaultsToTwo()
    {
        var qty = HistoryRetentionService.GetProtectedSentQty(new ReportSettingsModel(), new[]
        {
            (ReportSectionType.Content, new ReportSectionSettingsModel()),
        });

        qty.Should().Be(HistoryRetentionService.DefaultHistoryQty);
    }

    [Fact]
    public void ProtectedSentQty_DuplicateTitleRemovalKeepsTen()
    {
        var qty = HistoryRetentionService.GetProtectedSentQty(new ReportSettingsModel(), new[]
        {
            (ReportSectionType.Content, new ReportSectionSettingsModel() { RemoveDuplicateTitles3Days = true }),
        });

        qty.Should().Be(HistoryRetentionService.DuplicateTitleHistoryQty);
    }

    [Fact]
    public void ProtectedSentQty_ReportLevelDuplicateTitleRemovalKeepsTen()
    {
        var report = new ReportSettingsModel();
        report.Content.RemoveDuplicateTitles3Days = true;

        HistoryRetentionService.GetProtectedSentQty(report, Array.Empty<(ReportSectionType, ReportSectionSettingsModel)>())
            .Should().Be(HistoryRetentionService.DuplicateTitleHistoryQty);
    }

    [Fact]
    public void ProtectedSentQty_LargestAIPreviousReportsWins()
    {
        var qty = HistoryRetentionService.GetProtectedSentQty(new ReportSettingsModel(), new[]
        {
            (ReportSectionType.AI, new ReportSectionSettingsModel() { IncludePreviousReports = 14 }),
            (ReportSectionType.AI, new ReportSectionSettingsModel() { IncludePreviousReports = 3 }),
            (ReportSectionType.Content, new ReportSectionSettingsModel() { RemoveDuplicateTitles3Days = true }),
        });

        qty.Should().Be(14);
    }

    [Fact]
    public void ProtectedSentQty_IgnoresPreviousReportsOnNonAISections()
    {
        var qty = HistoryRetentionService.GetProtectedSentQty(new ReportSettingsModel(), new[]
        {
            (ReportSectionType.Content, new ReportSectionSettingsModel() { IncludePreviousReports = 30 }),
        });

        qty.Should().Be(HistoryRetentionService.DefaultHistoryQty);
    }
    #endregion

    #region Notification cutoff
    [Fact]
    public void NotificationCutoff_UsesRetention()
    {
        var now = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);

        HistoryRetentionService.GetNotificationCutoff(now, 30, null, new FilterSettingsModel())
            .Should().Be(now.AddDays(-30));
    }

    [Fact]
    public void NotificationCutoff_LongerFilterWindowWins()
    {
        var now = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);

        HistoryRetentionService.GetNotificationCutoff(now, 30, 5, new FilterSettingsModel() { DateOffset = 60 })
            .Should().Be(now.AddDays(-60));
    }

    [Fact]
    public void NotificationCutoff_LongerPublishedBeforeOffsetWins()
    {
        var now = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);

        HistoryRetentionService.GetNotificationCutoff(now, 30, 45, new FilterSettingsModel() { DateOffset = 2 })
            .Should().Be(now.AddDays(-45));
    }

    [Fact]
    public void NotificationCutoff_FixedStartDateKeepsEverythingSince()
    {
        var now = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        HistoryRetentionService.GetNotificationCutoff(now, 30, null, new FilterSettingsModel() { StartDate = start })
            .Should().Be(start);
    }

    [Fact]
    public void NotificationCutoff_RecentStartDateDoesNotShortenRetention()
    {
        var now = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);

        HistoryRetentionService.GetNotificationCutoff(now, 30, null, new FilterSettingsModel() { StartDate = now.AddDays(-1) })
            .Should().Be(now.AddDays(-30));
    }
    #endregion
}
