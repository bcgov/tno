using Microsoft.Extensions.DependencyInjection;
using TNO.Services.ContentMigration.Config;
using TNO.Services.Runners;
using TNO.Services.ContentMigration.Sources.Oracle.Services;
using TNO.Services.ContentMigration.Sources.Oracle;
using Oracle.ManagedDataAccess.Client;

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
            .AddMigrationSourceContext(this.Configuration)
            .AddTransient<IIngestAction<ContentMigrationOptions>, ContentMigrationAction>()
            .AddTransient<IngestManagerFactory<ContentMigrationIngestActionManager, ContentMigrationOptions>>()
            .AddSingleton<IServiceManager, ContentMigrationManager>();

        // services.AddTransient<OracleConnection>(provider => { var connectionString = "User Id=TNO_USER;Password='TnoVsMmia2023';Data Source=localhost:41521/freepdb1"; return new OracleConnection(connectionString); });

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ContentMigrationOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        // Build an intermediate service provider
        var sp = services.BuildServiceProvider();
        // This will succeed.
        var msg = sp.GetService<MigrationSourceContext>();

        return services;
    }
    #endregion
}
