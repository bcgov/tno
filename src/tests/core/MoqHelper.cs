using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TNO.Test.Core;

/// <summary>
/// MoqHelper static class, provides mock methods to speed up the setup of mocked objects.
/// </summary>
public static class MoqHelper
{
    /// <summary>
    /// Create and add the mocked type to the service collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMockSingleton<T>(this IServiceCollection services) where T : class
    {
        var mock = new Mock<T>();
        return services
          .AddSingleton(mock)
          .AddSingleton(mock.Object);
    }
}
