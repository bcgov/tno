using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Migrators;
using TNO.Services.ContentMigration.Sources.Oracle.Services;
using TNO.Services.Runners;

namespace TNO.Services.ContentMigration;

/// <summary>
/// ContentMigrationService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class ContentMigrationService : KafkaProducerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentMigrationService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ContentMigrationService(string[] args) : base(args)
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
            .Configure<ContentMigrationOptions>(this.Configuration.GetSection("Service"))
            .AddMigrationSourceContext(this.Configuration.GetSection("Service:OracleConnection").Get<OracleConnectionSettings>())
            .AddTransient<IIngestAction<ContentMigrationOptions>, ContentMigrationAction>()
            .AddTransient<IngestManagerFactory<ContentMigrationIngestActionManager, ContentMigrationOptions>>()
            .AddSingleton<IServiceManager, ContentMigrationManager>();

        services.AddSourceMigrators(this.Configuration);

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ContentMigrationOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
