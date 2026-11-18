using Xunit;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Subscriber.Controllers;
using TNO.API.Areas.Subscriber.Models.Report;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models.Settings;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;

namespace TNO.Test.API;

/// <summary>
/// The body of the chart preview names what to count, so it may only name what the user can see.
/// </summary>
public class ReportChartDataEndpointTest
{
    private const int UserId = 5;
    private readonly Mock<IReportService> _reports = new();
    private readonly Mock<IReportInstanceService> _instances = new();
    private readonly Mock<IReportHelper> _helper = new();
    private readonly Mock<IImpersonationHelper> _impersonate = new();
    private readonly Mock<IFilterService> _filters = new();
    private readonly Mock<IFolderService> _folders = new();
    private readonly Mock<IChartTemplateService> _charts = new();

    private ReportController CreateController(int ownerId = UserId, bool isPublic = false)
    {
        var report = new Report(1, "report", 1, ownerId) { IsPublic = isPublic };
        _reports.Setup(r => r.FindById(1)).Returns(report);
        _reports.Setup(r => r.GetLatestInstances(1, UserId, It.IsAny<int>())).Returns([]);
        _impersonate.Setup(i => i.GetCurrentUser()).Returns(new User("user", "user@test.com") { Id = UserId });
        _charts.Setup(c => c.FindById(2)).Returns(new ChartTemplate(2, "Custom", "") { Settings = JsonDocument.Parse("{\"aggregate\":true}") });
        return new ReportController(
            _reports.Object, _instances.Object, Mock.Of<IUserService>(), _helper.Object,
            Mock.Of<IKafkaMessenger>(), Options.Create(new KafkaOptions()), Options.Create(new KafkaHubConfig()), Mock.Of<IKafkaMessenger>(),
            _impersonate.Object, Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)), Mock.Of<ILogger<ReportController>>(),
            Mock.Of<ISettingService>(), Options.Create(new ChesOptions()), Mock.Of<IChesService>(), null!,
            _filters.Object, _folders.Object, _charts.Object);
    }

    [Fact]
    public async Task OtherUsersPrivateReport_IsRejected()
    {
        var controller = CreateController(ownerId: 99);
        await Assert.ThrowsAsync<NotAuthorizedException>(() => controller.GetChartDataAsync(1, 1, 2, new ChartDataPreviewRequestModel()));
    }

    [Fact]
    public async Task OtherUsersFilter_IsRejected()
    {
        var controller = CreateController();
        _filters.Setup(f => f.FindById(8)).Returns(new Filter(8, "theirs", 99));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => controller.GetChartDataAsync(1, 1, 2, new ChartDataPreviewRequestModel { FilterId = 8 }));
    }

    [Fact]
    public async Task OtherUsersFolder_IsRejected()
    {
        var controller = CreateController();
        _folders.Setup(f => f.FindById(8)).Returns(new Folder(8, "theirs", 99));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => controller.GetChartDataAsync(1, 1, 2, new ChartDataPreviewRequestModel { FolderId = 8 }));
    }

    [Fact]
    public async Task NoStoriesYet_ReturnsNoContent()
    {
        var controller = CreateController();
        var result = await controller.GetChartDataAsync(1, 1, 2, new ChartDataPreviewRequestModel { UseAllContent = true });
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PublicReport_ComputesWithTheDraftSettings_AndStoresNothing()
    {
        var controller = CreateController(ownerId: 99, isPublic: true);
        var settings = new ChartSectionSettingsModel { GroupBy = "source" };
        _filters.Setup(f => f.FindById(8)).Returns(new Filter(8, "mine", UserId) { Query = JsonDocument.Parse("{}") });
        _helper.Setup(h => h.ComputeDraftChartAsync(It.IsAny<Report>(), It.IsAny<ReportSection>(), It.IsAny<ReportSectionChartTemplate>(), settings, null, UserId))
            .ReturnsAsync(JsonDocument.Parse("""{"labels":["a"]}"""));

        var result = await controller.GetChartDataAsync(1, 1, 2, new ChartDataPreviewRequestModel { FilterId = 8, SectionSettings = settings });

        Assert.IsType<JsonResult>(result);
        _instances.Verify(i => i.SaveCharts(It.IsAny<long>(), It.IsAny<IEnumerable<ReportInstanceChart>>(), It.IsAny<int[]>()), Times.Never);
    }
}
