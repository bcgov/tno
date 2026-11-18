using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TNO.API.Areas.Services.Models.Notification;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Core.Exceptions;
using TNO.Elastic;
using TNO.Elastic.Models;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Controllers;
using TNO.Services.Notification;
using TNO.Services.Notification.Config;
using TNO.Services.Notification.Models;
using TNO.Services.Notification.Validation;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Config;
using ContentModel = TNO.API.Areas.Services.Models.Content.ContentModel;
using NotificationInstanceModel = TNO.API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace TNO.Test.Services;

public class NotificationRecoveryTest
{
    private const long ContentId = 9355361;

    private sealed class Clock : TimeProvider
    {
        private long _ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => _ticks;
        public void Advance(int seconds) => _ticks += TimeSpan.FromSeconds(seconds).Ticks;
    }

    private sealed class Manager : NotificationManager
    {
        public Manager(IKafkaListener<string, NotificationRequestModel> listener, IApiService api, INotificationValidator validator, IChesService ches, NotificationOptions options, Clock clock)
            : base(listener, api, new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "service@test.com")])), Engine(), ches,
                MsOptions.Create(new ChesOptions { From = "mmi@test.com" }), MsOptions.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                MsOptions.Create(options), MsOptions.Create(new TemplateOptions()), validator, NullLogger<NotificationManager>.Instance, clock)
        { }

        public Task Handle(ConsumeResult<string, NotificationRequestModel> message) => HandleMessageAsync(message);

        public Task RunOnce() => RunOnceAsync();

        private static INotificationEngine Engine()
        {
            var engine = new Mock<INotificationEngine>();
            engine.Setup(e => e.GenerateNotificationSubjectAsync(It.IsAny<NotificationModel>(), It.IsAny<TNO.TemplateEngine.Models.ContentModel>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync("subject");
            engine.Setup(e => e.GenerateNotificationBodyAsync(It.IsAny<NotificationModel>(), It.IsAny<TNO.TemplateEngine.Models.ContentModel>(), It.IsAny<string?>(), It.IsAny<bool>())).ReturnsAsync("body");
            return engine.Object;
        }
    }

    /// <summary>
    /// Everything a notification manager depends on, wired to in-memory fakes.
    /// </summary>
    private sealed class Harness : IDisposable
    {
        private readonly HttpClient _http = new();
        private int _sending;

        public Mock<IApiService> Api { get; } = new();
        public Mock<IKafkaListener<string, NotificationRequestModel>> Listener { get; } = new();
        public Mock<IChesService> Ches { get; } = new();
        public Mock<ITNOElasticClient> Elastic { get; } = new();
        public Clock Clock { get; } = new();
        public NotificationOptions Options { get; }
        public Manager Manager { get; }
        public List<NotificationModel> Notifications { get; } = [];
        public List<string> SentTo { get; } = [];
        public List<NotificationInstanceModel> Instances { get; } = [];
        public List<NotificationRequestModel> Requeued { get; } = [];
        public HashSet<string> FailFor { get; } = [];
        public int MostSendingAtOnce { get; private set; }

        public Harness(Action<NotificationOptions>? configure = null)
        {
            this.Options = new NotificationOptions { Topics = "notify", DefaultDelayMS = 0, RetryAfterCriticalFailureDelayMS = 0, ResendDelayMS = 0 };
            configure?.Invoke(this.Options);

            this.Api.Setup(a => a.OpenClient.Client).Returns(_http);
            this.Api.Setup(a => a.FindContentByIdAsync(ContentId, true)).ReturnsAsync(() => new ContentModel { Id = ContentId, Status = ContentStatus.Published });
            this.Api.Setup(a => a.GetNotificationsForAsync(ContentId)).ReturnsAsync([]);
            this.Api.Setup(a => a.FindNotificationsAsync(It.IsAny<TNO.Models.Filters.NotificationFilter>())).ReturnsAsync(() => this.Notifications);
            this.Api.Setup(a => a.GetNotificationAsync(It.IsAny<int>())).ReturnsAsync((int id) => this.Notifications.FirstOrDefault(n => n.Id == id));
            this.Api.Setup(a => a.AddNotificationInstanceAsync(It.IsAny<NotificationInstanceModel>()))
                .Callback((NotificationInstanceModel instance) => { lock (this.Instances) this.Instances.Add(instance); })
                .ReturnsAsync((NotificationInstanceModel instance) => instance);
            this.Api.Setup(a => a.SendMessageAsync(It.IsAny<NotificationRequestModel>()))
                .Callback((NotificationRequestModel request) => this.Requeued.Add(request))
                .ReturnsAsync((TNO.API.Areas.Kafka.Models.DeliveryResultModel<NotificationRequestModel>?)null);

            // A consumer that never receives anything, so starting it does not spin.
            this.Listener.Setup(l => l.ConsumeAsync(It.IsAny<Func<ConsumeResult<string, NotificationRequestModel>, Task>>(), It.IsAny<CancellationToken>()))
                .Returns(new TaskCompletionSource().Task);

            this.Ches.Setup(c => c.SendEmailAsync(It.IsAny<IEmailMerge>())).Returns(async (IEmailMerge email) =>
            {
                var to = email.Contexts.SelectMany(c => c.To).ToArray();
                var sending = Interlocked.Increment(ref _sending);
                lock (this.SentTo) this.MostSendingAtOnce = Math.Max(this.MostSendingAtOnce, sending);
                try
                {
                    await Task.Delay(10);
                    // The failure that stopped the service: not a ChesException.
                    if (to.Any(this.FailFor.Contains)) throw new HttpClientRequestException("HTTP Request failed [POST]:[RequestTimeout]", HttpStatusCode.RequestTimeout);
                    lock (this.SentTo) this.SentTo.AddRange(to);
                    return new EmailResponseModel();
                }
                finally
                {
                    Interlocked.Decrement(ref _sending);
                }
            });

            var validator = new NotificationValidator(this.Api.Object, this.Elastic.Object, MsOptions.Create(new ElasticOptions()), MsOptions.Create(this.Options), NullLogger<NotificationValidator>.Instance);
            this.Manager = new Manager(this.Listener.Object, this.Api.Object, validator, this.Ches.Object, this.Options, this.Clock);
        }

        public NotificationModel AddNotification(int id, params int[] subscriberIds)
        {
            var notification = new NotificationModel
            {
                Id = id,
                Name = $"Notification {id}",
                IsEnabled = true,
                AlertOnIndex = true,
                Resend = ResendOption.Updated,
                Template = new NotificationTemplateModel(),
                Subscribers = subscriberIds.Select(userId => new UserNotificationModel
                {
                    NotificationId = id,
                    UserId = userId,
                    IsSubscribed = true,
                    User = new UserModel { Id = userId, Email = Email(userId), DisplayName = $"User {userId}" },
                }).ToArray(),
            };
            this.Notifications.Add(notification);
            return notification;
        }

        public void Dispose() => _http.Dispose();
    }

    private static string Email(int userId) => $"user{userId}@test.com";

    private static ConsumeResult<string, NotificationRequestModel> Message(int? notificationId = null, NotificationRetryModel? retry = null) => new()
    {
        Topic = "notify",
        Partition = new Partition(0),
        Offset = new Offset(100),
        Message = new Message<string, NotificationRequestModel>
        {
            Key = $"content-{ContentId}",
            Value = new NotificationRequestModel
            {
                Destination = NotificationDestination.NotificationService,
                NotificationId = notificationId,
                ContentId = ContentId,
                IgnoreValidation = retry != null,
                Data = retry != null ? retry.ToData() : new { },
            },
        },
    };

    private static int Status(Manager manager) => Assert.IsType<JsonResult>(new HealthController(manager).Health()).StatusCode ?? 0;

    [Fact]
    public async Task FailedServiceResumesAndSubscribesAgain()
    {
        using var harness = new Harness(o => o.MaxFailureLimit = 1);
        harness.Manager.State.RecordFailure();
        Assert.Equal(ServiceStatus.RequestFailed, harness.Manager.State.Status);

        await harness.Manager.RunOnce();
        Assert.Equal(ServiceStatus.Failed, harness.Manager.State.Status);
        harness.Listener.Verify(l => l.Stop(), Times.Once);

        await harness.Manager.RunOnce();
        Assert.Equal(ServiceStatus.Running, harness.Manager.State.Status);
        Assert.Equal(0, harness.Manager.State.Failures);

        await harness.Manager.RunOnce();
        harness.Listener.Verify(l => l.Subscribe("notify"), Times.Once);
    }

    [Fact]
    public async Task FailedServiceStaysFailedWhenAutoRestartIsOff()
    {
        using var harness = new Harness(o => { o.MaxFailureLimit = 1; o.AutoRestartAfterCriticalFailure = false; });
        harness.Manager.State.RecordFailure();

        await harness.Manager.RunOnce();
        await harness.Manager.RunOnce();

        Assert.Equal(ServiceStatus.Failed, harness.Manager.State.Status);
        harness.Listener.Verify(l => l.Subscribe(It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HealthFailsOnlyWhenTheAutomaticResumeIsOverdue()
    {
        using var harness = new Harness(o => { o.MaxFailureLimit = 1; o.RetryAfterCriticalFailureDelayMS = 120000; o.DefaultDelayMS = 30000; });
        Assert.Equal(200, Status(harness.Manager));

        harness.Manager.State.RecordFailure();
        await harness.Manager.RunOnce();
        Assert.Equal(ServiceStatus.Failed, harness.Manager.State.Status);

        // The service resumes on its own within the retry delay plus the loop delay.
        harness.Clock.Advance(180);
        Assert.Equal(200, Status(harness.Manager));

        harness.Clock.Advance(1);
        Assert.Equal(503, Status(harness.Manager));
        Assert.Contains("Failed", harness.Manager.HealthFailureReason);

        harness.Manager.State.Resume();
        Assert.Equal(200, Status(harness.Manager));

        // A later failure is timed from when it happened, not from the first one.
        harness.Manager.State.RecordFailure();
        await harness.Manager.RunOnce();
        Assert.Equal(200, Status(harness.Manager));
    }

    [Fact]
    public async Task HealthFailsStraightAwayWhenNothingWillResumeTheService()
    {
        using var harness = new Harness(o => { o.MaxFailureLimit = 1; o.AutoRestartAfterCriticalFailure = false; });
        harness.Manager.State.RecordFailure();
        await harness.Manager.RunOnce();

        harness.Clock.Advance(1);
        Assert.Equal(503, Status(harness.Manager));
    }

    [Fact]
    public async Task NotificationWithAnInvalidFilterIsSkippedWithoutFailingTheOthers()
    {
        using var harness = new Harness(o => { o.MaxFailureLimit = 1; o.SendEmailOnFailure = true; o.EmailTo = "support@test.com"; });
        var invalid = harness.AddNotification(210, 1);
        invalid.Settings = new TNO.API.Models.Settings.FilterSettingsModel { Search = "\"Adrian Dix\" | \" Caroline Elliott", InHeadline = true };
        harness.AddNotification(37, 2);
        harness.Elastic.Setup(e => e.SearchAsync<ContentModel>(It.IsAny<string>(), It.IsAny<JsonDocument>()))
            .ThrowsAsync(new HttpClientRequestException("HTTP Request failed [POST]:[BadRequest]", HttpStatusCode.BadRequest));

        var message = Message();
        await harness.Manager.Handle(message);
        await harness.Manager.Handle(message);

        Assert.Equal([Email(2), Email(2)], harness.SentTo);
        Assert.Equal(ServiceStatus.Running, harness.Manager.State.Status);
        Assert.Equal(0, harness.Manager.State.Failures);
        harness.Listener.Verify(l => l.Commit(message), Times.Exactly(2));
        // The invalid filter is reported once, not for every content item.
        harness.Ches.Verify(c => c.SendEmailAsync(It.IsAny<IEmail>()), Times.Once);
    }

    [Fact]
    public async Task ElasticsearchOutageStillFailsTheMessage()
    {
        using var harness = new Harness();
        var notification = harness.AddNotification(210, 1);
        notification.Settings = new TNO.API.Models.Settings.FilterSettingsModel { Search = "housing", InHeadline = true };
        harness.Elastic.Setup(e => e.SearchAsync<ContentModel>(It.IsAny<string>(), It.IsAny<JsonDocument>()))
            .ThrowsAsync(new HttpClientRequestException("HTTP Request failed [POST]:[ServiceUnavailable]", HttpStatusCode.ServiceUnavailable));

        var message = Message();
        await harness.Manager.Handle(message);

        Assert.Equal(1, harness.Manager.State.Failures);
        harness.Listener.Verify(l => l.Commit(message), Times.Never);
    }

    [Fact]
    public async Task EmailThatCannotBeSentDoesNotStopTheOthersAndIsSentBackToKafka()
    {
        using var harness = new Harness();
        harness.AddNotification(3, 1, 2, 3);
        harness.FailFor.Add(Email(2));

        var message = Message();
        await harness.Manager.Handle(message);

        Assert.Equal([Email(1), Email(3)], harness.SentTo.Order());
        Assert.Equal(ServiceStatus.Running, harness.Manager.State.Status);
        Assert.Equal(0, harness.Manager.State.Failures);
        harness.Listener.Verify(l => l.Commit(message), Times.Once);

        // Only the emails CHES accepted are recorded; the failure is not final yet.
        Assert.Equal(2, harness.Instances.Count);
        Assert.All(harness.Instances, i => Assert.Equal(NotificationStatus.Accepted, i.Status));

        var requeued = Assert.Single(harness.Requeued);
        Assert.Equal(3, requeued.NotificationId);
        Assert.Equal(ContentId, requeued.ContentId);
        Assert.True(requeued.IgnoreValidation);
        var retry = NotificationRetryModel.From((object?)requeued.Data);
        Assert.NotNull(retry);
        Assert.Equal(1, retry.FailureCount);
        Assert.Equal([2], retry.UserIds.AsEnumerable());
        Assert.NotNull(retry.RetryAfter);
    }

    [Fact]
    public async Task EveryEmailIsSentBeforeTheMessageIsCommitted()
    {
        using var harness = new Harness(o => o.MaxConcurrentSends = 2);
        harness.AddNotification(3, 1, 2, 3, 4, 5, 6, 7, 8);
        var sentWhenCommitted = -1;
        harness.Listener.Setup(l => l.Commit(It.IsAny<ConsumeResult<string, NotificationRequestModel>>())).Callback(() => sentWhenCommitted = harness.SentTo.Count);

        await harness.Manager.Handle(Message());

        Assert.Equal(8, sentWhenCommitted);
        Assert.Equal(8, harness.Instances.Count);
        Assert.Equal(2, harness.MostSendingAtOnce);
    }

    [Fact]
    public async Task RetryIsOnlySentToTheSubscribersThatFailed()
    {
        using var harness = new Harness();
        harness.AddNotification(3, 1, 2, 3);

        await harness.Manager.Handle(Message(3, new NotificationRetryModel { FailureCount = 1, UserIds = [2], RetryAfter = DateTime.UtcNow.AddMinutes(-1) }));

        Assert.Equal([Email(2)], harness.SentTo);
        Assert.Empty(harness.Requeued);
        Assert.Equal(NotificationStatus.Accepted, Assert.Single(harness.Instances).Status);
    }

    [Fact]
    public async Task RetryThatFailsAgainIsSentBackWithTheNextAttempt()
    {
        using var harness = new Harness();
        harness.AddNotification(3, 1, 2, 3);
        harness.FailFor.Add(Email(2));

        await harness.Manager.Handle(Message(3, new NotificationRetryModel { FailureCount = 2, UserIds = [2] }));

        Assert.Empty(harness.Instances);
        var retry = NotificationRetryModel.From((object?)Assert.Single(harness.Requeued).Data);
        Assert.NotNull(retry);
        Assert.Equal(3, retry.FailureCount);
        Assert.Equal([2], retry.UserIds.AsEnumerable());
    }

    [Fact]
    public async Task FailureIsRecordedOnceTheAttemptLimitIsReached()
    {
        using var harness = new Harness();
        harness.AddNotification(3, 1, 2, 3);
        harness.FailFor.Add(Email(2));

        var message = Message(3, new NotificationRetryModel { FailureCount = 3, UserIds = [2] });
        await harness.Manager.Handle(message);

        Assert.Empty(harness.Requeued);
        Assert.Equal(NotificationStatus.Failed, Assert.Single(harness.Instances).Status);
        harness.Listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public async Task FailureIsRecordedWhenResendingIsOff()
    {
        using var harness = new Harness(o => o.ResendOnFailure = false);
        harness.AddNotification(3, 1, 2);
        harness.FailFor.Add(Email(2));

        await harness.Manager.Handle(Message());

        Assert.Empty(harness.Requeued);
        Assert.Equal([NotificationStatus.Accepted, NotificationStatus.Failed], harness.Instances.Select(i => i.Status).Order());
    }

    [Fact]
    public async Task FailureIsRecordedWhenItCannotBeSentBackToKafka()
    {
        using var harness = new Harness();
        harness.AddNotification(3, 1);
        harness.FailFor.Add(Email(1));
        harness.Api.Setup(a => a.SendMessageAsync(It.IsAny<NotificationRequestModel>())).ThrowsAsync(new HttpClientRequestException("API unavailable", HttpStatusCode.ServiceUnavailable));

        var message = Message();
        await harness.Manager.Handle(message);

        Assert.Equal(NotificationStatus.Failed, Assert.Single(harness.Instances).Status);
        harness.Listener.Verify(l => l.Commit(message), Times.Once);
    }

    [Fact]
    public void RetryStateIsOnlyReadFromARetryRequest()
    {
        Assert.Null(NotificationRetryModel.From(null));
        Assert.Null(NotificationRetryModel.From(new { }));
        Assert.Null(NotificationRetryModel.From(JsonSerializer.SerializeToElement(new { failureCount = "many" })));
        Assert.Null(NotificationRetryModel.From(JsonSerializer.SerializeToElement(new[] { 1, 2 })));

        var retry = NotificationRetryModel.From(JsonSerializer.SerializeToElement(new { failureCount = 2, userIds = new[] { 5, 9 }, retryAfter = "2026-11-18T12:30:00Z" }));
        Assert.NotNull(retry);
        Assert.Equal(2, retry.FailureCount);
        Assert.Equal([5, 9], retry.UserIds.AsEnumerable());
        Assert.Equal(new DateTime(2026, 11, 18, 12, 30, 0, DateTimeKind.Utc), retry.RetryAfter?.ToUniversalTime());
    }
}
