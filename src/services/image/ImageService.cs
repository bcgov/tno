using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Image.Config;
using TNO.Services.Runners;

namespace TNO.Services.Image;

/// <summary>
/// ImageService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class ImageService : IngestService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ImageService(string[] args) : base(args)
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
            .Configure<ImageOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<ImageOptions>, ImageAction>()
            .AddTransient<IngestManagerFactory<ImageIngestActionManager, ImageOptions>>()
            .AddSingleton<IServiceManager, ImageManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ImageOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
