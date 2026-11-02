using System.Text.Json;
using AwesomeAssertions;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.Kafka;

namespace TNO.Test.Services.Kafka;

/// <summary>
/// Runs against a real broker. Set TNO_TEST_KAFKA to a bootstrap server (e.g. localhost:40102); the test creates
/// and deletes its own topic.
/// </summary>
public class KafkaListenerSkipBacklogTest
{
    private static KafkaListener<string, string> CreateListener(string bootstrap, string group, bool skipBacklog) => new(
        Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)),
        Options.Create(new KafkaConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = group,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            ConsumeTimeoutMs = 500,
            // KafkaListener.Stop() unassigns rather than closes, so a short session lets the next listener in the group take over quickly.
            SessionTimeoutMs = 6000,
            SkipBacklogOnStart = skipBacklog,
        }),
        NullLogger<KafkaListener<string, string>>.Instance);

    private static async Task<List<string>> ConsumeAsync(KafkaListener<string, string> listener, int expected, TimeSpan timeout)
    {
        var received = new List<string>();
        var deadline = DateTime.UtcNow + timeout;
        while (received.Count < expected && DateTime.UtcNow < deadline)
        {
            await listener.ConsumeAsync(result =>
            {
                received.Add(result.Message.Value);
                listener.Commit(result);
                return Task.CompletedTask;
            }, CancellationToken.None);
        }
        return received;
    }

    [Fact]
    public async Task SkipBacklogOnStart_ConsumesOnlyMessagesSentAfterTheListenerStarts()
    {
        var bootstrap = Environment.GetEnvironmentVariable("TNO_TEST_KAFKA");
        Assert.SkipWhen(String.IsNullOrWhiteSpace(bootstrap), "Set TNO_TEST_KAFKA to a Kafka bootstrap server to run this test.");

        var topic = $"tno-test-backlog-{Guid.NewGuid():N}";
        var group = topic;
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrap }).Build();
        await admin.CreateTopicsAsync([new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }]);
        try
        {
            using var producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrap }).Build();
            for (var i = 1; i <= 5; i++) await producer.ProduceAsync(topic, new Message<string, string> { Key = $"{i}", Value = $"backlog-{i}" });

            // The listener starts after the backlog exists and skips it.
            using (var listener = CreateListener(bootstrap!, group, skipBacklog: true))
            {
                listener.Subscribe(topic);
                await ConsumeAsync(listener, 1, TimeSpan.FromSeconds(8)); // join the group; the backlog must not arrive
                await producer.ProduceAsync(topic, new Message<string, string> { Key = "6", Value = "new-6" });
                var received = await ConsumeAsync(listener, 1, TimeSpan.FromSeconds(20));
                received.Should().Equal("new-6");
                listener.Stop();
            }

            // Without the option the same group resumes from its committed offset as usual.
            await producer.ProduceAsync(topic, new Message<string, string> { Key = "7", Value = "new-7" });
            using (var listener = CreateListener(bootstrap!, group, skipBacklog: false))
            {
                listener.Subscribe(topic);
                var received = await ConsumeAsync(listener, 1, TimeSpan.FromSeconds(40));
                received.Should().Equal("new-7");
                listener.Stop();
            }
        }
        finally
        {
            await admin.DeleteTopicsAsync([topic]);
        }
    }
}
