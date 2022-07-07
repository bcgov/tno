using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TNO.Services.Config;
using TNO.Services.Controllers;
using TNO.Test.Core;

namespace TNO.Test.Services.Controllers;

public class HealthControllerTest
{
    [Fact]
    public void Health_Success()
    {
        // Arrange
        var helper = new TestHelper();
        var provider = helper.Build(services =>
        {
            services.AddMockSingleton<IServiceManager>();
            services.AddSingleton<HealthController>();
        });
        var controller = provider.GetRequiredService<HealthController>();

        var state = new ServiceState(new ServiceOptions());
        var mock = provider.GetRequiredService<Mock<IServiceManager>>();
        mock.Setup(m => m.State).Returns(state);

        // Act
        var result = controller.Health();

        // Assert
        Assert.IsType<JsonResult>(result);
        dynamic data = (result as JsonResult)?.Value!;
        Assert.Equal(ServiceStatus.Running, data.Status);
    }
}
