using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TNO.Kafka;

/// <summary>
/// ServiceCollectionExtensions static class, provides extension methods for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the KafkaMessenger to the service collection with the default configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaMessenger(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<ProducerConfig>(config.GetSection("Kafka:Producer"))
            .AddScoped<IKafkaMessenger, KafkaMessenger>();
    }
}
