using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TNO.Elastic.ExampleHosted;

/// <summary>
/// ExampleHostedService class, provides an example hosted service that stops immediately.
/// </summary>
public class ExampleHostedService : IHostedService
{
    #region Variables
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ExampleHostedService object, initializes with specified parameters.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="appLifetime"></param>
    public ExampleHostedService(ILogger<ExampleHostedService> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service starting");

        _appLifetime.StopApplication();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop the service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service stopping");
        return Task.FromResult(0);
    }
    #endregion
}
