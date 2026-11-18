using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Runners;
using TNO.Services.Scheduler.Config;

namespace TNO.Services.Scheduler;

/// <summary>
/// SchedulerService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class SchedulerService : BaseService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SchedulerService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public SchedulerService(string[] args) : base(args)
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
            .Configure<SchedulerOptions>(this.Configuration.GetSection("Service"))
            .AddSingleton<IServiceManager, SchedulerManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<SchedulerOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
