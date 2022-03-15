using Microsoft.EntityFrameworkCore;
using TNO.Tools.Import.Destination.Configuration;
using TNO.Tools.Import.Destination.Entities;
using TNO.Core.Extensions;

namespace TNO.Tools.Import.Destination;

public class DestinationContext : DbContext
{
    #region Properties
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<ContentType> ContentTypes => Set<ContentType>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<DataLocation> DataLocations => Set<DataLocation>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<TonePool> TonePools => Set<TonePool>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Entities.Action> Actions => Set<Entities.Action>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<FileReference> FileReferences => Set<FileReference>();
    public DbSet<ContentLink> ContentLinks => Set<ContentLink>();
    public DbSet<ContentTonePool> ContentTonePools => Set<ContentTonePool>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<ContentAction> ContentActions => Set<ContentAction>();
    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Claim> Claims => Set<Claim>();
    #endregion

    #region Constructors
    public DestinationContext()
    {

    }

    public DestinationContext(DbContextOptions<DestinationContext> options)
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
        modelBuilder.ApplyAllConfigurations(typeof(ContentConfiguration), this);
    }
    #endregion
}