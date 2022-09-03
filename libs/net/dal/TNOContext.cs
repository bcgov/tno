using Microsoft.EntityFrameworkCore;
using TNO.DAL.Configuration;
using TNO.Entities;
using TNO.Core.Extensions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.DAL.Extensions;
using Microsoft.Extensions.Logging;

namespace TNO.DAL;

public class TNOContext : DbContext
{
    #region Variables
    private readonly ILogger? _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly JsonSerializerOptions? _serializerOptions;
    #endregion

    #region Properties
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<ContentLog> ContentLogs => Set<ContentLog>();
    public DbSet<PrintContent> PrintContents => Set<PrintContent>();
    public DbSet<ContentReference> ContentReferences => Set<ContentReference>();
    public DbSet<ContentReferenceLog> ContentReferenceLogs => Set<ContentReferenceLog>();
    public DbSet<ContentLink> ContentLinks => Set<ContentLink>();
    public DbSet<FileReference> FileReferences => Set<FileReference>();
    public DbSet<TimeTracking> TimeTrackings => Set<TimeTracking>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<ContentType> ContentTypes => Set<ContentType>();
    public DbSet<ContentTypeAction> ContentTypeActions => Set<ContentTypeAction>();
    public DbSet<ContentAction> ContentActions => Set<ContentAction>();
    public DbSet<Entities.Action> Actions => Set<Entities.Action>();
    public DbSet<ContentLabel> ContentLabels => Set<ContentLabel>();
    public DbSet<ContentTonePool> ContentTonePools => Set<ContentTonePool>();
    public DbSet<TonePool> TonePools => Set<TonePool>();
    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<DataService> DataServices => Set<DataService>();
    public DbSet<DataLocation> DataLocations => Set<DataLocation>();
    public DbSet<DataSourceMetric> DataSourceMetrics => Set<DataSourceMetric>();
    public DbSet<SourceMetric> SourceMetrics => Set<SourceMetric>();
    public DbSet<DataSourceAction> DataSourceActions => Set<DataSourceAction>();
    public DbSet<SourceAction> SourceActions => Set<SourceAction>();
    public DbSet<DataSourceSchedule> DataSourceSchedules => Set<DataSourceSchedule>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Cache> Cache => Set<Cache>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();
    public DbSet<Claim> Claims => Set<Claim>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TNOContext object, initializes with specified parameters.
    /// </summary>
    /// <param name="logger"></param>
    protected TNOContext(ILogger<TNOContext> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new instance of a TNOContext object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public TNOContext(DbContextOptions<TNOContext> options, IHttpContextAccessor? httpContextAccessor = null, IOptions<JsonSerializerOptions>? serializerOptions = null, ILogger<TNOContext>? logger = null)
      : base(options)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _serializerOptions = serializerOptions?.Value;
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
        modelBuilder.ApplyAllConfigurations(typeof(ContentConfiguration), this);
    }

    /// <summary>
    /// Save the entities with who created them or updated them.
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges()
    {
        // get entries that are being Added or Updated
        var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

        var user = _httpContextAccessor?.HttpContext?.User;
        foreach (var entry in modifiedEntries)
        {
            if (entry.Entity is AuditColumns entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.OnAdded(user);
                }
                else if (entry.State != EntityState.Deleted)
                {
                    this.OnUpdate(entity, user);
                }
            }
        }

        return base.SaveChanges();
    }

    /// <summary>
    /// Wrap the save changes in a transaction for rollback.
    /// </summary>
    /// <returns></returns>
    public int CommitTransaction()
    {
        var result = 0;
        using (var transaction = this.Database.BeginTransaction())
        {
            try
            {
                result = this.SaveChanges();
                transaction.Commit();
            }
            catch (DbUpdateException)
            {
                transaction.Rollback();
                throw;
            }
        }
        return result;
    }

    /// <summary>
    /// Deserialize the specified 'json' to the specified type of 'T'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _serializerOptions);
    }

    /// <summary>
    /// Serialize the specified 'item'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public string Serialize<T>(T item)
    {
        return JsonSerializer.Serialize(item, _serializerOptions);
    }
    #endregion
}
