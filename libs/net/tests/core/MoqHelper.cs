using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TNO.Test.Core;

public static class MoqHelper
{

    public static IServiceCollection AddMock<T>(this IServiceCollection services) where T : class
    {
        return services.AddSingleton<T>(Mock.Of<T>());
    }
}
