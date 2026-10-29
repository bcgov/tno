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
        public Manager(IKafkaListener<string, ReportRequestModel> listener, IApiService api, Clock clock, bool retry = false)
            : base(listener, api, new ClaimsPrincipal(), Mock.Of<IReportEngine>(), Mock.Of<IChesService>(),
                Microsoft.Extensions.Options.Options.Create(new ChesOptions()), Microsoft.Extensions.Options.Options.Create(new JsonSerializerOptions()),
                Microsoft.Extensions.Options.Options.Create(new ReportingOptions { Topics = "reporting", ResendOnFailure = retry }), NullLogger<ReportingManager>.Instance, clock)
        { }

        public Task Handle(ConsumeResult<string, ReportRequestModel> message) => HandleMessageAsync(message);
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
}
