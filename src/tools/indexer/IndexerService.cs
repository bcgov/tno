using Microsoft.Extensions.DependencyInjection;
using TNO.DAL;
using TNO.Services;
using TNO.Services.Runners;
using TNO.Tools.ElasticIndexer.Config;

namespace TNO.Tools.ElasticIndexer;

public class IndexerService : BaseService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of an IndexerService.
    /// </summary>
    /// <param name="args"></param>
    public IndexerService(string[] args) : base(args)
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
            .Configure<IndexerOptions>(this.Configuration.GetSection("Service"))
            .AddSingletonTNOServices(this.Configuration, this.Environment)
            .AddSingleton<IServiceManager, IndexerManager>()
            .AddMemoryCache(
                options =>
                {
                    options.SizeLimit = 100;
                }
            );

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<IndexingOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
