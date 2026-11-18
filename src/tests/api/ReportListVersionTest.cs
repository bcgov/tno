using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Subscriber.Controllers;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Models.Filters;
using Xunit;

namespace TNO.Test.API;

/// <summary>
/// The app keeps the list of a user's reports while the page stays open, so it must be able to
/// ask whether the list is stale without the reports being fetched again.
/// </summary>
public class ReportListVersionTest
{
    private const int UserId = 5;
    private readonly Mock<IReportService> _reports = new();

    private ReportController CreateController(string version, string? ifNoneMatch = null)
    {
        _reports.Setup(r => r.GetVersion(It.Is<ReportFilter>(f => f.OwnerId == UserId))).Returns(version);
        _reports.Setup(r => r.Find(It.IsAny<ReportFilter>(), true)).Returns([new Report(1, "report", 1, UserId)]);
        var impersonate = new Mock<IImpersonationHelper>();
        impersonate.Setup(i => i.GetCurrentUser()).Returns(new User("user", "user@test.com") { Id = UserId });

        var http = new DefaultHttpContext();
        http.Request.Scheme = "http";
        http.Request.Host = new HostString("localhost");
        http.Request.Path = "/api/subscriber/reports/my-reports";
        if (ifNoneMatch != null) http.Request.Headers.IfNoneMatch = ifNoneMatch;

        return new ReportController(
            _reports.Object, Mock.Of<IReportInstanceService>(), Mock.Of<IUserService>(), Mock.Of<IReportHelper>(),
            Mock.Of<IKafkaMessenger>(), Options.Create(new KafkaOptions()), Options.Create(new KafkaHubConfig()), Mock.Of<IKafkaMessenger>(),
            impersonate.Object, Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)), Mock.Of<ILogger<ReportController>>(),
            Mock.Of<ISettingService>(), Options.Create(new ChesOptions()), Mock.Of<IChesService>(), null!,
            Mock.Of<IFilterService>(), Mock.Of<IFolderService>(), Mock.Of<IChartTemplateService>())
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
    }

    [Fact]
    public void FirstRequest_ReturnsTheReports_WithTheirVersion()
    {
        var controller = CreateController("abc");

        var result = controller.FindMyReports();

        Assert.IsType<JsonResult>(result);
        Assert.Equal("\"5-abc\"", controller.Response.Headers.ETag.ToString());
        // Without this a browser may reuse the list without asking whether it changed.
        Assert.Equal("private, no-cache", controller.Response.Headers.CacheControl.ToString());
    }

    [Theory]
    [InlineData("\"5-abc\"")]
    // A proxy that compresses the response marks the version as weak.
    [InlineData("W/\"5-abc\"")]
    [InlineData("\"5-old\", \"5-abc\"")]
    public void SameVersion_IsNotModified_AndTheReportsAreNotRead(string ifNoneMatch)
    {
        var controller = CreateController("abc", ifNoneMatch);

        var result = Assert.IsType<StatusCodeResult>(controller.FindMyReports());

        Assert.Equal(StatusCodes.Status304NotModified, result.StatusCode);
        Assert.Equal("\"5-abc\"", controller.Response.Headers.ETag.ToString());
        _reports.Verify(r => r.Find(It.IsAny<ReportFilter>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [InlineData("\"5-old\"")]
    // Another user's list with the same contents is still not this user's list.
    [InlineData("\"6-abc\"")]
    [InlineData("")]
    public void DifferentVersion_ReturnsTheReports(string ifNoneMatch)
    {
        var controller = CreateController("abc", ifNoneMatch);

        Assert.IsType<JsonResult>(controller.FindMyReports());
        _reports.Verify(r => r.Find(It.IsAny<ReportFilter>(), true), Times.Once);
    }
}
