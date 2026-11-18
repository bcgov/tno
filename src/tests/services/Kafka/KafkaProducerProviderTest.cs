using System.Text.Json;
using AwesomeAssertions;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.Test.Services.Kafka;

public class KafkaProducerProviderTest
{
    /// <summary>
    /// Building a producer does not connect, so these tests need no broker.
    /// </summary>
    private static KafkaProducerProvider CreateProvider(string bootstrapServers = "127.0.0.1:1") => new(
        Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)),
        Options.Create(new ProducerConfig { BootstrapServers = bootstrapServers, EnableIdempotence = true, Acks = Acks.All, MessageTimeoutMs = 10000 }),
        NullLogger<KafkaProducerProvider>.Instance);

    [Fact]
    public void GetProducer_ReturnsTheSameProducerForTheSameTypes()
    {
        using var provider = CreateProvider();

        provider.GetProducer<string, IndexRequestModel>().Should().BeSameAs(provider.GetProducer<string, IndexRequestModel>());
    }

    [Fact]
    public void GetProducer_EveryTypeSharesOneClient()
    {
        using var provider = CreateProvider();

        var index = provider.GetProducer<string, IndexRequestModel>();
        var report = provider.GetProducer<string, ReportRequestModel>();

        index.Should().NotBeSameAs(report);
        // The client name (rdkafka#producer-N) identifies the underlying librdkafka client.
        index.Name.Should().Be(report.Name);
    }

    [Fact]
    public void Reset_CreatesANewClientOnTheNextSend()
    {
        using var provider = CreateProvider();
        var before = provider.GetProducer<string, IndexRequestModel>();
        var beforeName = before.Name;

        provider.Reset();
        var after = provider.GetProducer<string, IndexRequestModel>();

        after.Should().NotBeSameAs(before);
        after.Name.Should().NotBe(beforeName);
    }

    [Fact]
    public void Dispose_PreventsFurtherUse()
    {
        var provider = CreateProvider();
        provider.GetProducer<string, IndexRequestModel>();

        provider.Dispose();
        provider.Dispose();

        provider.Invoking(p => p.GetProducer<string, IndexRequestModel>()).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task SendMessageAsync_UsesTheProviderProducerForEveryMessage()
    {
        var producer = new Mock<IProducer<string, IndexRequestModel>>();
        producer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, IndexRequestModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, IndexRequestModel>());
        var provider = new Mock<IKafkaProducerProvider>();
        provider.Setup(p => p.GetProducer<string, IndexRequestModel>()).Returns(producer.Object);
        var messenger = new KafkaMessenger(provider.Object, NullLogger<KafkaMessenger>.Instance);

        await messenger.SendMessageAsync("index", new IndexRequestModel(1, null, IndexAction.Index));
        await messenger.SendMessageAsync("index", new IndexRequestModel(2, null, IndexAction.Index));

        producer.Verify(p => p.ProduceAsync("index", It.Is<Message<string, IndexRequestModel>>(m => m.Key == "1"), It.IsAny<CancellationToken>()), Times.Once);
        producer.Verify(p => p.ProduceAsync("index", It.Is<Message<string, IndexRequestModel>>(m => m.Key == "2"), It.IsAny<CancellationToken>()), Times.Once);
        provider.Verify(p => p.Reset(), Times.Never);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task SendMessageAsync_ResetsTheProducerOnlyAfterAFatalError(bool isFatal, int resets)
    {
        var error = new Error(isFatal ? ErrorCode.Local_Fatal : ErrorCode.Local_MsgTimedOut, "send failed", isFatal);
        var producer = new Mock<IProducer<string, IndexRequestModel>>();
        producer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, IndexRequestModel>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<string, IndexRequestModel>(error, new DeliveryResult<string, IndexRequestModel>()));
        var provider = new Mock<IKafkaProducerProvider>();
        provider.Setup(p => p.GetProducer<string, IndexRequestModel>()).Returns(producer.Object);
        var messenger = new KafkaMessenger(provider.Object, NullLogger<KafkaMessenger>.Instance);

        var send = () => messenger.SendMessageAsync("index", new IndexRequestModel(1, null, IndexAction.Index));

        await send.Should().ThrowAsync<ProduceException<string, IndexRequestModel>>();
        provider.Verify(p => p.Reset(), Times.Exactly(resets));
    }

    /// <summary>
    /// Sends concurrently through one shared producer to a real broker. Set TNO_TEST_KAFKA to a bootstrap
    /// server (e.g. localhost:40102) to run it; it creates and deletes its own topic.
    /// </summary>
    [Fact]
    public async Task SendMessageAsync_DeliversConcurrentMessagesThroughOneProducer()
    {
        var bootstrap = Environment.GetEnvironmentVariable("TNO_TEST_KAFKA");
        Assert.SkipWhen(String.IsNullOrWhiteSpace(bootstrap), "Set TNO_TEST_KAFKA to a Kafka bootstrap server to run this test.");

        var topic = $"tno-test-producer-{Guid.NewGuid():N}";
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrap }).Build();
        await admin.CreateTopicsAsync([new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }]);
        try
        {
            using var provider = CreateProvider(bootstrap!);
            var messenger = new KafkaMessenger(provider, NullLogger<KafkaMessenger>.Instance);

            var results = await Task.WhenAll(Enumerable.Range(1, 50).Select(i =>
                messenger.SendMessageAsync(topic, new IndexRequestModel(i, null, IndexAction.Index))));

            results.Should().AllSatisfy(r => r!.Status.Should().Be(PersistenceStatus.Persisted));
            results.Select(r => r!.Message.Key).Should().BeEquivalentTo(Enumerable.Range(1, 50).Select(i => $"{i}"));
            provider.GetProducer<string, IndexRequestModel>().Name.Should().Be(provider.GetProducer<string, ReportRequestModel>().Name);

            using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = topic,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            }).Build();
            consumer.Subscribe(topic);
            var received = new List<ConsumeResult<string, string>>();
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (received.Count < 50 && DateTime.UtcNow < deadline)
            {
                var message = consumer.Consume(TimeSpan.FromSeconds(1));
                if (message != null) received.Add(message);
            }
            received.Should().HaveCount(50);
            // Same JSON wire format as before: the string key is serialized as a JSON string.
            received[0].Message.Key.Should().StartWith("\"").And.EndWith("\"");
        }
        finally
        {
            await admin.DeleteTopicsAsync([topic]);
        }
    }
}
