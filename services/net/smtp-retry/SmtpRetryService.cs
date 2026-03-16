using Microsoft.Extensions.DependencyInjection;
using MMI.Services.SmtpRetry.Config;
using TNO.Services;
using TNO.Services.Runners;

namespace MMI.Services.SmtpRetry;

/// <summary>
/// SmtpRetryService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class SmtpRetryService : BaseService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SmtpRetryService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public SmtpRetryService(string[] args) : base(args)
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
            .Configure<SmtpRetryOptions>(this.Configuration.GetSection("Service"))
            .AddSingleton<IServiceManager, SmtpRetryManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<SmtpRetryOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
