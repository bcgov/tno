using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.Core.Exceptions;
using TNO.Core.Reporting;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using Xunit;
using Controller = TNO.API.Areas.Subscriber.Controllers.ReportInstanceController;

namespace TNO.Test.API;

public class ReportSizeValidationTest
{
    private static ReportInstance Instance()
    {
        var report = new Report(12, "Report", 1, 1);
        report.Sections.Add(new ReportSection("enabled", ReportSectionType.Content, report) { IsEnabled = true });
        report.Sections.Add(new ReportSection("disabled", ReportSectionType.Content, report) { IsEnabled = false });
        return new ReportInstance(31, report.Id, 1) { Report = report, Status = ReportStatus.Pending };
    }

    private static Controller Create(Mock<IReportInstanceService> service, Mock<IKafkaMessenger> kafka, int userId = 1)
    {
        var impersonation = new Mock<IImpersonationHelper>();
        impersonation.Setup(i => i.GetCurrentUser()).Returns(new User("owner", "owner@example.test") { Id = userId });
        return new Controller(Mock.Of<IReportService>(), service.Object, Mock.Of<IUserService>(), impersonation.Object,
            Mock.Of<IReportHelper>(), kafka.Object, Options.Create(new KafkaOptions()), null!, Options.Create(new JsonSerializerOptions()));
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("send")]
    [InlineData("preview")]
    public async Task OversizedReportReturnsActionableFailureWithoutQueueingOrChangingContent(string operation)
    {
        var instance = Instance();
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(instance);
        service.Setup(s => s.EstimateReportSize(31, It.Is<string[]>(names => names.Length == 1 && names[0] == "enabled"), It.IsAny<string[]>()))
            .Returns(ReportSizeLimit.MaximumCharacters + 1L);
        var kafka = new Mock<IKafkaMessenger>(MockBehavior.Strict);
        var controller = Create(service, kafka);
        var error = await Assert.ThrowsAsync<ReportTooLargeException>(() => operation switch
        {
            "publish" => controller.PublishAsync(31, false),
            "send" => controller.SendToAsync(31, "owner@example.test"),
            _ => controller.ViewAsync(31, true),
        });
        Assert.Contains("Remove stories or narrow", error.Message);
        Assert.Equal(ReportStatus.Pending, instance.Status);
        service.Verify(s => s.GetContentForInstance(It.IsAny<long>()), Times.Never);
        service.Verify(s => s.UpdateAndSave(It.IsAny<ReportInstance>(), It.IsAny<bool>()), Times.Never);
        Assert.Empty(kafka.Invocations);
    }

    [Fact]
    public async Task UnauthorizedSendIsRejectedBeforeSizeIsQueried()
    {
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(Instance());
        await Assert.ThrowsAsync<NotAuthorizedException>(() => Create(service, new Mock<IKafkaMessenger>(), 2).PublishAsync(31, false));
        service.Verify(s => s.EstimateReportSize(It.IsAny<long>(), It.IsAny<string[]>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task AReportCanBeSentImmediatelyAfterItsContentIsReduced()
    {
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(Instance());
        service.SetupSequence(s => s.EstimateReportSize(31, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .Returns(ReportSizeLimit.MaximumCharacters + 1L).Returns(1000);
        var kafka = new Mock<IKafkaMessenger>();
        var controller = Create(service, kafka);
        await Assert.ThrowsAsync<ReportTooLargeException>(() => controller.SendToAsync(31, "owner@example.test"));
        Assert.Empty(kafka.Invocations);
        await controller.SendToAsync(31, "owner@example.test");
        Assert.Single(kafka.Invocations);
    }

    [Theory]
    [InlineData(false, "Saved subject")]
    [InlineData(true, "")]
    public async Task OutputThatWillBeRegeneratedCannotBypassPreflight(bool resend, string subject)
    {
        var instance = Instance();
        instance.Status = ReportStatus.Reopen;
        instance.Subject = subject;
        instance.Body = "Old small output";
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(instance);
        service.Setup(s => s.EstimateReportSize(31, It.IsAny<string[]>(), It.IsAny<string[]>())).Returns(ReportSizeLimit.MaximumCharacters + 1L);
        var kafka = new Mock<IKafkaMessenger>(MockBehavior.Strict);
        await Assert.ThrowsAsync<ReportTooLargeException>(() => Create(service, kafka).PublishAsync(31, resend));
        Assert.Empty(kafka.Invocations);
    }

    [Fact]
    public void OnlyEnabledFullStorySectionsCountTheStoryBody()
    {
        var instance = Instance();
        instance.Report!.Sections.First(s => s.Name == "enabled").Settings = JsonDocument.Parse("{\"showFullStory\":true}");
        instance.Report.Sections.Add(new ReportSection("headlines", ReportSectionType.Content, instance.Report) { IsEnabled = true, Settings = JsonDocument.Parse("{\"showFullStory\":false}") });
        instance.Report.Sections.First(s => s.Name == "disabled").Settings = JsonDocument.Parse("{\"showFullStory\":true}");
        var service = new Mock<IReportInstanceService>(MockBehavior.Strict);
        service.Setup(s => s.EstimateReportSize(31,
            It.Is<string[]>(names => names.SequenceEqual(new[] { "enabled", "headlines" })),
            It.Is<string[]>(names => names.SequenceEqual(new[] { "enabled" })))).Returns(1000);
        service.Object.ValidateReportSize(instance);
        service.VerifyAll();
    }

    [Fact]
    public void ReducedContentIsRecheckedAndStoredOutputIsCheckedWhenReused()
    {
        var instance = Instance();
        var service = new Mock<IReportInstanceService>();
        service.SetupSequence(s => s.EstimateReportSize(31, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .Returns(ReportSizeLimit.MaximumCharacters + 1L).Returns(1000);
        Assert.Throws<ReportTooLargeException>(() => service.Object.ValidateReportSize(instance));
        service.Object.ValidateReportSize(instance);
        instance.Body = new string('x', ReportSizeLimit.MaximumCharacters + 1);
        Assert.Throws<ReportTooLargeException>(() => service.Object.ValidateReportSize(instance, true));
    }

    private static ReportInstance CollectionOnlyInstance()
    {
        var instance = Instance();
        instance.Report!.Settings = JsonDocument.Parse("{\"collectionOnly\":true}");
        return instance;
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("send")]
    public async Task ACollectionOnlyReportCannotBeSent(string operation)
    {
        var instance = CollectionOnlyInstance();
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(instance);
        var kafka = new Mock<IKafkaMessenger>(MockBehavior.Strict);
        var controller = Create(service, kafka);
        var error = await Assert.ThrowsAsync<BadRequestException>(() => operation == "publish"
            ? controller.PublishAsync(31, false)
            : controller.SendToAsync(31, "owner@example.test"));
        Assert.Equal(ReportSizeValidation.CollectionOnlySendMessage, error.Message);
        Assert.Equal(ReportStatus.Pending, instance.Status);
        service.Verify(s => s.UpdateAndSave(It.IsAny<ReportInstance>(), It.IsAny<bool>()), Times.Never);
        Assert.Empty(kafka.Invocations);
    }

    [Fact]
    public async Task ACollectionOnlyPreviewIsRebuiltFromHeadlinesEvenWithLargeStoredOutput()
    {
        var instance = CollectionOnlyInstance();
        instance.Body = new string('x', ReportSizeLimit.MaximumCharacters + 1);
        var service = new Mock<IReportInstanceService>();
        service.Setup(s => s.FindById(31)).Returns(instance);
        service.Setup(s => s.EstimateHeadlineListSize(31, It.Is<string[]>(names => names.SequenceEqual(new[] { "enabled" })))).Returns(1000);
        var helper = new Mock<IReportHelper>();
        var impersonation = new Mock<IImpersonationHelper>();
        impersonation.Setup(i => i.GetCurrentUser()).Returns(new User("owner", "owner@example.test") { Id = 1 });
        var controller = new Controller(Mock.Of<IReportService>(), service.Object, Mock.Of<IUserService>(), impersonation.Object,
            helper.Object, Mock.Of<IKafkaMessenger>(), Options.Create(new KafkaOptions()), null!, Options.Create(new JsonSerializerOptions()));

        await controller.ViewAsync(31, false);

        // The stored output is never reused; the full-report estimate is never consulted.
        helper.Verify(h => h.PreviewReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), 1, It.IsAny<bool>()), Times.Once);
        service.Verify(s => s.EstimateReportSize(It.IsAny<long>(), It.IsAny<string[]>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public void ACollectionOnlyReportIsLimitedByItsHeadlineList()
    {
        var instance = CollectionOnlyInstance();
        var service = new Mock<IReportInstanceService>(MockBehavior.Strict);
        service.SetupSequence(s => s.EstimateHeadlineListSize(31, It.IsAny<string[]>()))
            .Returns(ReportSizeLimit.MaximumCharacters + 1L).Returns(1000);
        Assert.Throws<ReportTooLargeException>(() => service.Object.ValidateReportSize(instance));
        service.Object.ValidateReportSize(instance, true);
    }

    [Theory]
    [InlineData("{}", false)]
    [InlineData("{\"collectionOnly\":false}", false)]
    [InlineData("{\"collectionOnly\":\"true\"}", false)]
    [InlineData("[]", false)]
    [InlineData("{\"collectionOnly\":true}", true)]
    public void CollectionOnlyIsReadFromTheReportSettings(string settings, bool expected)
    {
        var report = new Report(12, "Report", 1, 1) { Settings = JsonDocument.Parse(settings) };
        Assert.Equal(expected, report.IsCollectionOnly());
        Assert.False(((Report?)null).IsCollectionOnly());
    }

    [Theory]
    [InlineData("admin-send")]
    [InlineData("admin-publish")]
    [InlineData("admin-instance-publish")]
    [InlineData("editor-publish")]
    public async Task EditorAndAdminCannotSendACollectionOnlyReport(string operation)
    {
        var instance = CollectionOnlyInstance();
        var reports = new Mock<IReportService>();
        reports.Setup(s => s.FindById(12)).Returns(instance.Report);
        var instances = new Mock<IReportInstanceService>();
        instances.Setup(s => s.FindById(31)).Returns(instance);
        var kafka = new Mock<IKafkaMessenger>(MockBehavior.Strict);
        var kafkaOptions = Options.Create(new KafkaOptions());
        var json = Options.Create(new JsonSerializerOptions());

        var error = await Assert.ThrowsAsync<BadRequestException>(() => operation switch
        {
            "admin-send" => new TNO.API.Areas.Admin.Controllers.ReportController(reports.Object, instances.Object, Mock.Of<IUserService>(),
                Mock.Of<IReportHelper>(), kafka.Object, null!, null!, null!, kafkaOptions, null!, json).SendToAsync(12, "owner@example.test"),
            "admin-publish" => new TNO.API.Areas.Admin.Controllers.ReportController(reports.Object, instances.Object, Mock.Of<IUserService>(),
                Mock.Of<IReportHelper>(), kafka.Object, null!, null!, null!, kafkaOptions, null!, json).Publish(12),
            "admin-instance-publish" => new TNO.API.Areas.Admin.Controllers.ReportInstanceController(instances.Object, Mock.Of<IUserService>(),
                kafka.Object, kafkaOptions, json).Publish(31),
            _ => new TNO.API.Areas.Editor.Controllers.ReportController(reports.Object, Mock.Of<IUserService>(), Mock.Of<IReportHelper>(),
                kafka.Object, kafkaOptions, null!, json).Publish(12),
        });

        Assert.Equal(ReportSizeValidation.CollectionOnlySendMessage, error.Message);
        Assert.Equal(ReportStatus.Pending, instance.Status);
        Assert.Empty(kafka.Invocations);
    }
}
