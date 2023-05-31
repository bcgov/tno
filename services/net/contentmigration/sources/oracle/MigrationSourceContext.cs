using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;

namespace TNO.Services.ContentMigration.Sources.Oracle;

public class MigrationSourceContext : DbContext
{
    #region Properties
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    #endregion

    #region Constructors
    public MigrationSourceContext()
    {

    }

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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyAllConfigurations(typeof(NewsItemConfiguration), this);
    }
    #endregion
}
