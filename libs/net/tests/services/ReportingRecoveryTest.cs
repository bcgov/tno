using System.Security.Claims;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services;
using TNO.Services.Controllers;
using TNO.Services.Reporting;
using TNO.Services.Reporting.Config;
using TNO.TemplateEngine;

namespace TNO.Test.Services;

public class ReportingRecoveryTest
{
    private sealed class Clock : TimeProvider
    {
        private long _ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => _ticks;
        public void Advance(int seconds) => _ticks += TimeSpan.FromSeconds(seconds).Ticks;
    }

    private static ConsumeResult<string, ReportRequestModel> Message(long offset = 100) => new()
    {
        Topic = "reporting",
        Partition = new Partition(0),
        Offset = new Offset(offset),
        Message = new Message<string, ReportRequestModel>
        {
            Key = "report-591",
            Value = new ReportRequestModel { ReportId = 591, ReportInstanceId = 66842, Destination = ReportDestination.ReportingService },
        },
    };

    [Fact]
    public void EmptyQueueRemainsHealthyForHoursWhilePolling()
    {
        var clock = new Clock();
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions(), clock);
        for (var second = 0; second < 7200; second++)
        {
            clock.Advance(1);
            watchdog.PollCompleted();
            Assert.Null(watchdog.Check(true, false, false));
        }
    }

    [Fact]
    public void PausedWithoutWorkFailsEvenWhenPollingContinues()
    {
        var clock = new Clock();
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions(), clock);
        Assert.Null(watchdog.Check(true, true, false));
        clock.Advance(59);
        watchdog.PollCompleted();
        Assert.Null(watchdog.Check(true, true, false));
        clock.Advance(1);
        watchdog.PollCompleted();
        Assert.Contains("paused without", watchdog.Check(true, true, false));
    }

    [Fact]
    public void BusyReportUsesItsLongerDeadlineAndLateCompletionCannotClearFailure()
    {
        var clock = new Clock();
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions(), clock);
        var message = Message();
        watchdog.Started(message);
        watchdog.Check(true, true, false);
        clock.Advance(1799);
        watchdog.PollCompleted();
        Assert.Null(watchdog.Check(true, true, false));
        watchdog.Progress("Sending report emails");
        clock.Advance(1);
        watchdog.PollCompleted();
        var failure = watchdog.Check(true, true, false);
        Assert.Contains("ReportId:591 InstanceId:66842", failure);
        Assert.Contains("Stage:Sending report emails SecondsSinceProgress:1", failure);
        watchdog.Completed(message.TopicPartitionOffset);
        Assert.Equal(failure, watchdog.Check(true, false, false));
    }

    [Fact]
    public void ConsumerThatStopsPollingFailsWithoutNeedingQueueLag()
    {
        var clock = new Clock();
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions(), clock);
        watchdog.Check(true, false, false);
        clock.Advance(60);
        Assert.Contains("stopped polling", watchdog.Check(true, false, false));
    }

    [Fact]
    public void CompletedConsumerTaskIsUnhealthy()
    {
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions());
        watchdog.PollCompleted();
        Assert.Contains("task stopped", watchdog.Check(true, false, true));
    }

    [Fact]
    public void IntentionalPauseDoesNotFailAndResumeGetsAStartupGracePeriod()
    {
        var clock = new Clock();
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions(), clock);
        watchdog.PollCompleted();
        Assert.Null(watchdog.Check(false, true, true));
        clock.Advance(7200);
        Assert.Null(watchdog.Check(false, true, true));
        Assert.Null(watchdog.Check(true, false, true));
        watchdog.PollCompleted();
        Assert.Null(watchdog.Check(true, false, false));
    }

    [Fact]
    public void OverlappingMessagesFailClosedInsteadOfAllowingOutOfOrderCommits()
    {
        var watchdog = new ReportProcessingWatchdog(new ReportingOptions());
        Assert.True(watchdog.Started(Message()));
        Assert.False(watchdog.Started(Message(101)));
        Assert.Contains("Overlapping", watchdog.Check(true, true, false));
    }

    [Theory]
    [InlineData(0, 1800)]
    [InlineData(60, -1)]
    public void InvalidDeadlinesAreRejected(int stall, int report)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReportProcessingWatchdog(new ReportingOptions
        {
            ConsumerStallTimeoutSeconds = stall,
            ReportProcessingTimeoutSeconds = report,
        }));
    }

    private sealed class Manager : ReportingManager
    {
        public Manager(IKafkaListener<string, ReportRequestModel> listener, IApiService api, Clock clock, bool retry = false,
            IReportEngine? engine = null, IChesService? ches = null)
            : base(listener, api, new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "service@test.com")])), engine ?? Mock.Of<IReportEngine>(), ches ?? Mock.Of<IChesService>(),
                Microsoft.Extensions.Options.Options.Create(new ChesOptions()), Microsoft.Extensions.Options.Options.Create(new JsonSerializerOptions()),
                Microsoft.Extensions.Options.Options.Create(new ReportingOptions { Topics = "reporting", ResendOnFailure = retry, RetryDelayMS = 0 }), NullLogger<ReportingManager>.Instance, clock)
        { }

        public Task Handle(ConsumeResult<string, ReportRequestModel> message) => HandleMessageAsync(message);
    }

    [Fact]
    public async Task OversizedReportIsAcknowledgedWithoutRetryingTheSameContent()
    {
        using var http = new HttpClient();
        var api = new Mock<IApiService>();
        api.Setup(a => a.OpenClient.Client).Returns(http);
        api.Setup(a => a.GetReportInstanceAsync(It.IsAny<long>())).ThrowsAsync(
            new ReportingException(591, 66842, ReportingErrors.FailedToGenerateOutput, "Report failed",
                new TNO.Core.Exceptions.ReportTooLargeException()));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        await manager.Handle(Message());
        api.Verify(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>()), Times.Never);
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Once);
        listener.Verify(l => l.Resume(), Times.Once);
        Assert.Null(manager.HealthFailureReason);
    }

    [Fact]
    public async Task CommitFailureReturns503AndNeverResumesOrCommitsLaterMessages()
    {
        using var http = new HttpClient();
        var api = new Mock<IApiService>();
        api.SetupGet(a => a.OpenClient.Client).Returns(http);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        listener.Setup(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()))
            .Throws(new KafkaException(new Error(ErrorCode.IllegalGeneration)));
        var manager = new Manager(listener.Object, api.Object, new Clock());

        await manager.Handle(Message());
        await manager.Handle(Message(101));

        Assert.Contains("Kafka commit failed", manager.HealthFailureReason);
        Assert.Equal(503, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Once);
        listener.Verify(l => l.Resume(), Times.Never);
        api.Verify(a => a.GetReportInstanceAsync(66842), Times.Once);
        // The generic runner may retry this singleton; that must not clear the failure.
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RunAsync());
        Assert.Equal(503, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
    }

    [Fact]
    public async Task SuccessfulMessageCommitsResumesAndRemainsHealthy()
    {
        using var http = new HttpClient();
        var api = Mock.Of<IApiService>(a => a.OpenClient.Client == http);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api, new Clock());
        await manager.Handle(Message());
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Once);
        listener.Verify(l => l.Resume(), Times.Once);
        Assert.Equal(200, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
    }

    [Fact]
    public async Task ResumeFailureAlsoRequiresRestart()
    {
        using var http = new HttpClient();
        var api = Mock.Of<IApiService>(a => a.OpenClient.Client == http);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        listener.Setup(l => l.Resume()).Throws(new KafkaException(new Error(ErrorCode.Local_State)));
        var manager = new Manager(listener.Object, api, new Clock());
        await manager.Handle(Message());
        Assert.Contains("Kafka resume failed", manager.HealthFailureReason);
        Assert.Equal(503, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
    }

    [Fact]
    public async Task HungReportFailsHealthAndLateCompletionDoesNotCommit()
    {
        using var http = new HttpClient();
        var api = new Mock<IApiService>();
        api.SetupGet(a => a.OpenClient.Client).Returns(http);
        var pending = new TaskCompletionSource<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel?>();
        api.Setup(a => a.GetReportInstanceAsync(66842)).Returns(pending.Task);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var clock = new Clock();
        var manager = new Manager(listener.Object, api.Object, clock);
        var handling = manager.Handle(Message());
        clock.Advance(1800);
        Assert.Contains("deadline exceeded", manager.HealthFailureReason);
        Assert.Equal(503, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
        pending.SetResult(null);
        await handling;
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Never);
        listener.Verify(l => l.Resume(), Times.Never);
    }

    [Fact]
    public async Task FailedRetryPublicationLeavesOriginalOffsetUncommitted()
    {
        using var http = new HttpClient();
        var api = new Mock<IApiService>();
        api.SetupGet(a => a.OpenClient.Client).Returns(http);
        api.Setup(a => a.GetReportInstanceAsync(66842)).ThrowsAsync(new HttpRequestException("API unavailable"));
        api.Setup(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>())).ThrowsAsync(new HttpRequestException("Queue unavailable"));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), retry: true);

        await Assert.ThrowsAsync<HttpRequestException>(() => manager.Handle(Message()));

        Assert.Contains("did not complete", manager.HealthFailureReason);
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Never);
        listener.Verify(l => l.Resume(), Times.Never);
    }

    [Fact]
    public void FatalConsumerErrorFailsHealthImmediately()
    {
        using var http = new HttpClient();
        var api = Mock.Of<IApiService>(a => a.OpenClient.Client == http);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api, new Clock());
        listener.Raise(l => l.OnError += null, listener.Object,
            new ErrorEventArgs(new KafkaException(new Error(ErrorCode.Local_Fatal, "Fatal consumer failure", true))));
        Assert.Contains("Fatal Kafka consumer error", manager.HealthFailureReason);
        Assert.Equal(503, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
    }

    [Fact]
    public void IntentionalServicePauseKeepsLivenessHealthy()
    {
        using var http = new HttpClient();
        var api = Mock.Of<IApiService>(a => a.OpenClient.Client == http);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var clock = new Clock();
        var manager = new Manager(listener.Object, api, clock);
        manager.State.Pause();
        manager.State.Stop();
        clock.Advance(3600);
        Assert.Equal(200, Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode);
    }

    [Fact]
    public async Task OverlappingDeliveryCannotCommitPastAnActiveReport()
    {
        using var http = new HttpClient();
        var api = new Mock<IApiService>();
        api.SetupGet(a => a.OpenClient.Client).Returns(http);
        var pending = new TaskCompletionSource<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel?>();
        api.Setup(a => a.GetReportInstanceAsync(66842)).Returns(pending.Task);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock());

        var first = manager.Handle(Message());
        await manager.Handle(Message(101));
        Assert.Contains("Overlapping", manager.HealthFailureReason);
        pending.SetResult(null);
        await first;

        api.Verify(a => a.GetReportInstanceAsync(66842), Times.Once);
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Never);
        listener.Verify(l => l.Resume(), Times.Never);
    }

    [Theory]
    // The request is marked with the instance its output was saved to, so whatever failed afterwards reuses it.
    [InlineData("{\"error\":5,\"outputInstanceId\":66842}", true)]
    [InlineData("{\"error\":4,\"outputInstanceId\":66842}", true)]
    [InlineData("{\"failureCount\":1,\"outputInstanceId\":66842}", true)]
    // The instance holds output from an earlier send, which this request did not save.
    [InlineData("{\"error\":5}", false)]
    [InlineData("{\"error\":3}", false)]
    [InlineData("{\"failureCount\":1,\"outputInstanceId\":null}", false)]
    [InlineData("{\"error\":5,\"outputInstanceId\":66841}", false)]
    public void RetryReusesOutputOnlyWhenTheRequestSavedIt(string data, bool expected)
    {
        var request = new ReportRequestModel { Data = JsonDocument.Parse(data) };
        var instance = new TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel { Id = 66842, Subject = "old subject", Body = "old output" };
        var method = typeof(ReportingManager).GetMethod("IsRetry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        Assert.Equal(expected, (bool)method.Invoke(null, [request, instance])!);
    }

    /// <summary>
    /// An API holding a report with one subscriber and an unsent instance of it.
    /// </summary>
    private static (Mock<IApiService> Api, TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel Instance) SendingApi(HttpClient http)
    {
        var report = new TNO.API.Areas.Services.Models.Report.ReportModel
        {
            Id = 591,
            Name = "Daily",
            Subscribers =
            [
                new TNO.API.Areas.Services.Models.Report.UserReportModel
                {
                    UserId = 5,
                    ReportId = 591,
                    IsSubscribed = true,
                    Format = TNO.Entities.ReportDistributionFormat.FullText,
                    User = new TNO.API.Areas.Services.Models.Report.UserModel { Id = 5, Email = "subscriber@test.com" },
                },
            ],
        };
        var instance = new TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel { Id = 66842, ReportId = 591, Report = report, Version = 1 };
        var api = new Mock<IApiService>();
        api.Setup(a => a.OpenClient.Client).Returns(http);
        api.Setup(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(instance);
        api.Setup(a => a.GetContentForReportInstanceIdAsync(66842)).ReturnsAsync([]);
        api.Setup(a => a.GetReportInstanceChartsAsync(66842, It.IsAny<int?>())).ReturnsAsync([]);
        api.Setup(a => a.GetUserReportInstancesAsync(66842)).ReturnsAsync([]);
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), false))
            .ReturnsAsync((TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel model, bool _) => model);
        return (api, instance);
    }

    [Fact]
    public async Task FailureAfterTheOutputIsSavedMarksTheRetryToReuseIt()
    {
        using var http = new HttpClient();
        var (api, instance) = SendingApi(http);
        var engine = new Mock<IReportEngine>();
        engine.SetReturnsDefault(Task.FromResult("generated"));
        var ches = new Mock<IChesService>();
        ches.SetReturnsDefault(Task.FromException<TNO.Ches.Models.EmailResponseModel>(new TNO.Core.Exceptions.ChesException("CHES is down")));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true, engine.Object, ches.Object);
        var message = Message();
        message.Message.Value.SendToSubscribers = true;

        await manager.Handle(message);

        ches.Verify(c => c.SendEmailAsync(It.IsAny<TNO.Ches.Models.IEmail>()), Times.AtLeastOnce);
        Assert.Equal("generated", instance.Subject);
        Assert.Equal("generated", instance.Body);
        api.Verify(a => a.SendMessageAsync(It.Is<ReportRequestModel>(r => r.ReportInstanceId == 66842
            && r.Data.RootElement.GetProperty("outputInstanceId").GetInt64() == 66842
            && r.Data.RootElement.GetProperty("failureCount").GetInt32() == 1)), Times.Once);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task RetryOfARequestThatSavedItsOutputSendsItWithoutGeneratingItAgain()
    {
        using var http = new HttpClient();
        var (api, instance) = SendingApi(http);
        instance.Subject = "saved subject";
        instance.Body = "saved body";
        var engine = new Mock<IReportEngine>();
        engine.SetReturnsDefault(Task.FromResult("generated"));
        var sent = new List<string>();
        var ches = new Mock<IChesService>();
        ches.Setup(c => c.SendEmailAsync(It.IsAny<TNO.Ches.Models.IEmail>()))
            .Callback<TNO.Ches.Models.IEmail>(email => sent.Add($"{email.Subject}|{email.Body}"))
            .ReturnsAsync(new TNO.Ches.Models.EmailResponseModel());
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true, engine.Object, ches.Object);
        var message = Message();
        message.Message.Value.SendToSubscribers = true;
        // The failure was not an email failure: the emails went out and a later step failed.
        message.Message.Value.Data = JsonDocument.Parse("{\"failureCount\":1,\"outputInstanceId\":66842}");

        await manager.Handle(message);

        Assert.Equal(["saved subject|saved body"], sent);
        Assert.DoesNotContain(engine.Invocations, i => i.Method.Name == nameof(IReportEngine.GenerateReportSubjectAsync));
        api.Verify(a => a.GetContentForReportInstanceIdAsync(It.IsAny<long>()), Times.Never);
        api.Verify(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>()), Times.Never);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task CollectionFailureDropsTheSavedOutputMark()
    {
        using var http = new HttpClient();
        var api = CollectionApi(http);
        api.Setup(a => a.GetCurrentReportInstanceAsync(591, It.IsAny<int?>())).ThrowsAsync(new HttpRequestException("Bad gateway"));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        var message = CollectionMessage();
        message.Message.Value.Data = JsonDocument.Parse("{\"outputInstanceId\":66842}");

        await manager.Handle(message);

        api.Verify(a => a.SendMessageAsync(It.Is<ReportRequestModel>(r => r.ReportInstanceId == null
            && r.Data.RootElement.GetProperty("outputInstanceId").ValueKind == JsonValueKind.Null)), Times.Once);
    }

    /// <summary>
    /// An API holding a collection-only report whose current instance is at version 10.
    /// </summary>
    private static Mock<IApiService> CollectionApi(HttpClient http)
    {
        var report = new TNO.API.Areas.Services.Models.Report.ReportModel
        {
            Id = 591,
            Settings = new TNO.API.Models.Settings.ReportSettingsModel { CollectionOnly = true },
        };
        var api = new Mock<IApiService>();
        api.Setup(a => a.OpenClient.Client).Returns(http);
        api.Setup(a => a.GetReportAsync(591)).ReturnsAsync(report);
        api.Setup(a => a.GetCurrentReportInstanceAsync(591, It.IsAny<int?>()))
            .ReturnsAsync(new TNO.API.Areas.Services.Models.Report.ReportInstanceModel { Id = 66842, ReportId = 591, Version = 10 });
        api.Setup(a => a.GenerateReportInstanceChartsAsync(66842, It.IsAny<int?>())).ReturnsAsync([]);
        // Saving the status afterwards succeeds.
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), false))
            .ReturnsAsync((TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel model, bool _) => model);
        return api;
    }

    private static TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel Instance(long version) => new() { Id = 66842, ReportId = 591, Version = version };

    private static ConsumeResult<string, ReportRequestModel> CollectionMessage()
    {
        var message = Message();
        message.Message.Value.ReportInstanceId = null;
        return message;
    }

    [Fact]
    public async Task CollectionWriteSavedByAFailedRequestIsNotSentAgain()
    {
        using var http = new HttpClient();
        var api = CollectionApi(http);
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true))
            .ThrowsAsync(new HttpRequestException("Response lost after commit"));
        // The version moved, so the request that failed was saved.
        api.Setup(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(() => Instance(11));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        var message = CollectionMessage();

        await manager.Handle(message);

        api.Verify(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true), Times.Once);
        api.Verify(a => a.GenerateReportInstanceChartsAsync(66842, It.IsAny<int?>()), Times.Once);
        api.Verify(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>()), Times.Never);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task CollectionWriteThatWasNotSavedIsSentAgain()
    {
        using var http = new HttpClient();
        var api = CollectionApi(http);
        api.SetupSequence(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true))
            .ThrowsAsync(new HttpRequestException("Bad gateway"))
            .ReturnsAsync(Instance(11));
        // The version has not moved, so nothing was saved.
        api.SetupSequence(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(Instance(10)).ReturnsAsync(Instance(11));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        var message = CollectionMessage();

        await manager.Handle(message);

        api.Verify(a => a.UpdateReportInstanceAsync(It.Is<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(i => i.Version == 10), true), Times.Exactly(2));
        api.Verify(a => a.GenerateReportInstanceChartsAsync(66842, It.IsAny<int?>()), Times.Once);
        api.Verify(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>()), Times.Never);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task CollectionWriteThatKeepsFailingGoesBackToCollection()
    {
        using var http = new HttpClient();
        var api = CollectionApi(http);
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true))
            .ThrowsAsync(new HttpRequestException("Bad gateway"));
        api.Setup(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(() => Instance(10));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        var message = CollectionMessage();

        await manager.Handle(message);

        api.Verify(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true), Times.Exactly(new ReportingOptions().RetryLimit));
        api.Verify(a => a.SendMessageAsync(It.Is<ReportRequestModel>(r => r.ReportId == 591 && r.ReportInstanceId == null)), Times.Once);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task CollectionWriteRejectedAsStaleGoesBackToCollectionWithoutBeingSentAgain()
    {
        using var http = new HttpClient();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"type\":\"DbUpdateConcurrencyException\"}", System.Text.Encoding.UTF8, "application/json"),
        };
        var api = CollectionApi(http);
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true))
            .ThrowsAsync(new TNO.Core.Exceptions.HttpClientRequestException(response));
        // Another change moved the version; this request saved nothing.
        api.Setup(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(() => Instance(11));
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true);
        var message = CollectionMessage();

        await manager.Handle(message);

        api.Verify(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), true), Times.Once);
        api.Verify(a => a.GetReportInstanceAsync(It.IsAny<long>()), Times.Never);
        api.Verify(a => a.GenerateReportInstanceChartsAsync(It.IsAny<long>(), It.IsAny<int?>()), Times.Never);
        api.Verify(a => a.SendMessageAsync(It.Is<ReportRequestModel>(r => r.ReportId == 591 && r.ReportInstanceId == null)), Times.Once);
        listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task CollectionOnlyReportIsCollectedButNeverRenderedOrSent()
    {
        using var http = new HttpClient();
        var report = new TNO.API.Areas.Services.Models.Report.ReportModel
        {
            Id = 591,
            Name = "Trade War Categories",
            Settings = new TNO.API.Models.Settings.ReportSettingsModel { CollectionOnly = true },
        };
        var instance = new TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel
        {
            Id = 66842,
            ReportId = 591,
            Report = report,
            Status = TNO.Entities.ReportStatus.Pending,
            // Output left from before the report was collection-only, larger than the limit.
            Body = new string('x', TNO.Core.Reporting.ReportSizeLimit.MaximumCharacters + 1),
        };
        TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel? saved = null;
        var api = new Mock<IApiService>();
        api.Setup(a => a.OpenClient.Client).Returns(http);
        api.Setup(a => a.GetReportInstanceAsync(66842)).ReturnsAsync(instance);
        api.Setup(a => a.GetContentForReportInstanceIdAsync(66842)).ReturnsAsync([]);
        api.Setup(a => a.GenerateReportInstanceChartsAsync(66842, It.IsAny<int?>())).ReturnsAsync([]);
        api.Setup(a => a.UpdateReportInstanceAsync(It.IsAny<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(), It.IsAny<bool>()))
            .Callback<TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel, bool>((model, _) => saved = model)
            .ReturnsAsync((TNO.API.Areas.Services.Models.ReportInstance.ReportInstanceModel model, bool _) => model);
        var engine = new Mock<IReportEngine>(MockBehavior.Strict);
        var ches = new Mock<IChesService>(MockBehavior.Strict);
        var listener = new Mock<IKafkaListener<string, ReportRequestModel>>();
        var manager = new Manager(listener.Object, api.Object, new Clock(), true, engine.Object, ches.Object);

        await manager.Handle(Message());

        Assert.NotNull(saved);
        Assert.Equal(TNO.Entities.ReportStatus.Completed, saved!.Status);
        // Not marked sent, so the next run keeps collecting into the same instance.
        Assert.Null(saved.SentOn);
        Assert.Equal("", saved.Body);
        api.Verify(a => a.GetContentForReportInstanceIdAsync(It.IsAny<long>()), Times.Never);
        api.Verify(a => a.GenerateReportInstanceChartsAsync(66842, It.IsAny<int?>()), Times.Once);
        Assert.Empty(engine.Invocations);
        Assert.Empty(ches.Invocations);
        api.Verify(a => a.SendMessageAsync(It.IsAny<ReportRequestModel>()), Times.Never);
        listener.Verify(l => l.Commit(It.IsAny<ConsumeResult<string, ReportRequestModel>>()), Times.Once);
        Assert.Null(manager.HealthFailureReason);
    }
}
