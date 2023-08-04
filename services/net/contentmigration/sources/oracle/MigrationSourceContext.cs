using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TNO.Core.Extensions;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// MigrationSourceContext class, provides an entity to store News Item records in the database.
/// </summary>
public class MigrationSourceContext : DbContext
{
    private readonly IConfiguration configuration;
    private string Schema => configuration.GetValue("Service:OracleConnection:DefaultSchema", "");

    #region Properties
    /// <summary>
    /// get/set for Set of NewsItem.
    /// </summary>
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();

    /// <summary>
    /// get/set for Set of UsersTones.
    /// </summary>
    public DbSet<UserTone> UsersTones => Set<UserTone>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationSourceContext object, initializes with specified parameters.
    /// </summary>
    public MigrationSourceContext()
    {

    }

    /// <summary>
    ///  Updates the connection string used to connect to the underlying database
    /// </summary>
    /// <param name="connectionString"></param>
    public void ChangeDatabaseConnectionString(string connectionString)
    {
        this.Database.SetConnectionString(connectionString);
    }

    /// <summary>
    /// Creates a new instance of a MigrationSourceContext object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public MigrationSourceContext(DbContextOptions<MigrationSourceContext> options, IConfiguration configuration)
      : base(options)
    {
        this.configuration = configuration;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Configures the DbContext with the specified options.
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }


    /// <summary>
    /// Apply all the configuration settings to the model.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        if (string.IsNullOrWhiteSpace(Schema))
        {
            modelBuilder.Entity<NewsItem>()
            .ToSqlQuery("WITH ATN AS (SELECT * from NEWS_ITEMS UNION ALL SELECT * from HNEWS_ITEMS) SELECT * FROM ATN");
        }
        else
        {
            modelBuilder.Entity<NewsItem>()
            .ToSqlQuery($"WITH ATN AS (SELECT * from {Schema}.NEWS_ITEMS UNION ALL SELECT * from {Schema}.HNEWS_ITEMS) SELECT * FROM ATN");
        }

        modelBuilder.ApplyAllConfigurations(typeof(NewsItemConfiguration), this);
        modelBuilder.ApplyAllConfigurations(typeof(UserToneConfiguration), this);

        modelBuilder.Entity<NewsItem>()
            .HasMany(s => s.Tones)
            .WithOne()
            .HasForeignKey(e => e.ItemRSN);

        modelBuilder.Entity<NewsItem>()
            .Navigation(e => e.Tones)
            .AutoInclude();
    }
    #endregion
}
