using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.Kafka;

namespace TNO.Test.Services;

public class KafkaListenerRecoveryTest
{
    private sealed class Listener : KafkaListener<string, string>
    {
        private readonly IConsumer<string, string> _consumer;

        public Listener(IConsumer<string, string> consumer, KafkaConsumerConfig config)
            : base(Options.Create(new JsonSerializerOptions()), Options.Create(config), NullLogger<KafkaListener<string, string>>.Instance)
        {
            _consumer = consumer;
        }

        protected override IConsumer<string, string> Build() => _consumer;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailedCommitReleasesTheHandlerSlotExactlyOnce(bool stopOnFailure)
    {
        var consumer = new Mock<IConsumer<string, string>>();
        consumer.SetupGet(c => c.Assignment).Returns(new List<TopicPartition> { new("reporting", 0) });
        var result = new ConsumeResult<string, string>
        {
            Topic = "reporting",
            Partition = 0,
            Offset = 1,
            Message = new Message<string, string> { Key = "report", Value = "data" },
        };
        consumer.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(result);
        consumer.Setup(c => c.Commit(result)).Throws(new KafkaException(new Error(ErrorCode.IllegalGeneration)));
        using var listener = new Listener(consumer.Object, new KafkaConsumerConfig
        {
            MaxThreads = 2,
            ReleaseHandlerOnCommitFailure = stopOnFailure,
        });
        listener.IsLongRunningJob = true;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await listener.ConsumeAsync(message =>
        {
            Assert.Throws<KafkaException>(() => listener.Commit(message));
            if (!stopOnFailure) listener.Seek(message);
            completed.SetResult();
            return Task.CompletedTask;
        }, CancellationToken.None);
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var second = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await listener.ConsumeAsync(_ => { second.SetResult(); return Task.CompletedTask; }, CancellationToken.None);
        await second.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // With a leaked slot, the second message would reach MaxThreads and pause the consumer.
        consumer.Verify(c => c.Pause(It.IsAny<IEnumerable<TopicPartition>>()), Times.Never);

        var third = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await listener.ConsumeAsync(_ => { third.SetResult(); return Task.CompletedTask; }, CancellationToken.None);
        await third.Task.WaitAsync(TimeSpan.FromSeconds(5));
        // Releasing the first slot twice would incorrectly allow this extra message without pausing.
        consumer.Verify(c => c.Pause(It.IsAny<IEnumerable<TopicPartition>>()), Times.Once);
    }

    [Fact]
    public async Task BoundedEmptyPollReturnsWithoutDispatchingAReport()
    {
        var consumer = new Mock<IConsumer<string, string>>();
        using var listener = new Listener(consumer.Object, new KafkaConsumerConfig { ConsumeTimeoutMs = 1000 });
        var dispatched = false;
        await listener.ConsumeAsync(_ => { dispatched = true; return Task.CompletedTask; }, CancellationToken.None);
        Assert.False(dispatched);
        consumer.Verify(c => c.Consume(TimeSpan.FromSeconds(1)), Times.Once);
        consumer.Verify(c => c.Consume(It.IsAny<CancellationToken>()), Times.Never);
    }
}
