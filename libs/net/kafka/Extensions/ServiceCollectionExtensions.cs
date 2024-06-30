using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka.SignalR;

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

    /// <summary>
    /// Add the KafkaListener to the service collection with the default configuration.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaListener(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<KafkaConsumerConfig>(config.GetSection("Kafka:Consumer"))
            .Configure<ConsumerConfig>(config.GetSection("Kafka:Consumer"))
            .AddScoped(typeof(IKafkaListener<,>), typeof(KafkaListener<,>));
    }

    /// <summary>
    /// Add the KafkaHub to the service collection with the default configuration.
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaHubBackPlane(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<KafkaHubConfig>(config.GetSection("Kafka"))
            .Configure<KafkaConsumerConfig>(config.GetSection("Kafka:Consumer"))
            .Configure<ConsumerConfig>(config.GetSection("Kafka:Consumer"))
            .Configure<ProducerConfig>(config.GetSection("Kafka:Producer"))
            .AddSingleton<IUserIdProvider, HubUsernameProvider>()
            .AddSingleton(typeof(HubLifetimeManager<>), typeof(KafkaHubLifetimeManager<>));
    }
}
