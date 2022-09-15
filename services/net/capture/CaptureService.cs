using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Capture.Config;
using TNO.Services.Command;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class CaptureService : CommandService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public CaptureService(string[] args) : base(args)
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
            .Configure<CaptureOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<CaptureOptions>, CaptureAction>()
            .AddTransient<IngestManagerFactory<CaptureIngestActionManager, CaptureOptions>>()
            .AddSingleton<IServiceManager, CaptureManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<CaptureOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
