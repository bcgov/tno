using TNO.Services.Config;

namespace TNO.Test.Services;

public class ServiceStateTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        var options = new ServiceOptions()
        {
            MaxFailureLimit = 10
        };

        // Act
        var state = new ServiceState(options);

        // Assert
        Assert.Equal(options.MaxFailureLimit, state.MaxFailureLimit);
        Assert.Equal(0, state.Failures);
        Assert.Equal(ServiceStatus.Running, state.Status);
    }

    [Fact]
    public void RecordFailure()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        var failures = state.RecordFailure();

        // Assert
        Assert.Equal(1, failures);
        Assert.Equal(1, state.Failures);
    }

    [Fact]
    public void RecordFailure_ToMaxFailureLimit()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        while (state.Failures < state.MaxFailureLimit)
            state.RecordFailure();

        // Assert
        Assert.Equal(state.MaxFailureLimit, state.Failures);
        Assert.Equal(ServiceStatus.RequestFailed, state.Status);
    }

    [Fact]
    public void ResetFailures()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.RecordFailure();
        state.ResetFailures();

        // Assert
        Assert.Equal(0, state.Failures);
    }

    [Fact]
    public void Sleep()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.Sleep();

        // Assert
        Assert.Equal(ServiceStatus.RequestSleep, state.Status);
    }

    [Fact]
    public void Pause()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.Pause();

        // Assert
        Assert.Equal(ServiceStatus.RequestPause, state.Status);
    }

    [Fact]
    public void Stop_Running()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.Stop();

        // Assert
        Assert.Equal(ServiceStatus.RequestSleep, state.Status);
    }

    [Fact]
    public void Stop_Sleep()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.Sleep();
        state.Stop();

        // Assert
        Assert.Equal(ServiceStatus.Sleeping, state.Status);
    }

    [Fact]
    public void Stop_Pause()
    {
        // Arrange
        var state = new ServiceState(new ServiceOptions());

        // Act
        state.Pause();
        state.Stop();

        // Assert
        Assert.Equal(ServiceStatus.Paused, state.Status);
    }
}
