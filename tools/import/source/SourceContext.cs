using Microsoft.EntityFrameworkCore;
using TNO.Tools.Import.Source.Configuration;
using TNO.Tools.Import.Source.Entities;
using TNO.Core.Extensions;

namespace TNO.Tools.Import.Source;

public class SourceContext : DbContext
{
    #region Properties
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    #endregion

    #region Constructors
    public SourceContext()
    {

    }

    public SourceContext(DbContextOptions<SourceContext> options)
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