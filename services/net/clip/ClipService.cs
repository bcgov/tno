using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Clip.Config;
using TNO.Services.Command;

namespace TNO.Services.Clip;

/// <summary>
/// ClipService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class ClipService : CommandService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClipService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ClipService(string[] args) : base(args)
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
            .Configure<ClipOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<ClipOptions>, ClipAction>()
            .AddTransient<IngestManagerFactory<ClipIngestActionManager, ClipOptions>>()
            .AddSingleton<IServiceManager, ClipManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ClipOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
