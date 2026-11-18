using Microsoft.Extensions.DependencyInjection;
using TNO.Services.ChesRetry.Config;
using TNO.Services.Runners;

namespace TNO.Services.ChesRetry;

/// <summary>
/// ChesRetryService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class ChesRetryService : BaseService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChesRetryService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ChesRetryService(string[] args) : base(args)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Configure dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .Configure<ChesRetryOptions>(this.Configuration.GetSection("Service"))
            .AddSingleton<IServiceManager, ChesRetryManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ChesRetryOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
