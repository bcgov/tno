using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// MigrationSourceContext class, provides an entity to store News Item records in the database.
/// </summary>
public class MigrationSourceContext : DbContext
{
    #region Properties
    /// <summary>
    /// get/set for Set of NewsItem.
    /// </summary>
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationSourceContext object, initializes with specified parameters.
    /// </summary>
    public MigrationSourceContext()
    {

    }

    /// <summary>
    /// Creates a new instance of a MigrationSourceContext object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public MigrationSourceContext(DbContextOptions<MigrationSourceContext> options)
      : base(options)
    {

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

        base.OnConfiguring(optionsBuilder);
    }


    /// <summary>
    /// Apply all the configuration settings to the model.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyAllConfigurations(typeof(NewsItemConfiguration), this);
    }
    #endregion
}
