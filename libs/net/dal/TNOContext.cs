using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.DAL.Configuration;
using TNO.DAL.Extensions;
using TNO.Entities;

namespace TNO.DAL;

public class TNOContext : DbContext
{
    #region Variables
    private readonly ILogger? _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly JsonSerializerOptions? _serializerOptions;
    #endregion

    #region Properties
    #region Admin
    public DbSet<SystemMessage> SystemMessages => Set<SystemMessage>();
    public DbSet<Cache> Cache => Set<Cache>();
    public DbSet<TopicScoreRule> TopicScoreRules => Set<TopicScoreRule>();
    public DbSet<SourceMetric> SourceMetrics => Set<SourceMetric>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Sentiment> Sentiments => Set<Sentiment>();
    public DbSet<EarnedMedia> EarnedMedias => Set<EarnedMedia>();
    public DbSet<Minister> Ministers => Set<Minister>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Setting> Settings => Set<Setting>();
    #endregion

    #region Ingest
    public DbSet<IngestType> IngestTypes => Set<IngestType>();
    public DbSet<Ingest> Ingests => Set<Ingest>();
    public DbSet<IngestState> IngestStates => Set<IngestState>();
    public DbSet<IngestSchedule> IngestSchedules => Set<IngestSchedule>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<DataLocation> DataLocations => Set<DataLocation>();
    public DbSet<IngestDataLocation> IngestDataLocations => Set<IngestDataLocation>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    #endregion

    #region Content
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<ContentLog> ContentLogs => Set<ContentLog>();
    public DbSet<ContentReference> ContentReferences => Set<ContentReference>();
    public DbSet<ContentLink> ContentLinks => Set<ContentLink>();
    public DbSet<FileReference> FileReferences => Set<FileReference>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<ContentTopic> ContentTopics => Set<ContentTopic>();
    public DbSet<ContentLabel> ContentLabels => Set<ContentLabel>();
    public DbSet<ContentTonePool> ContentTonePools => Set<ContentTonePool>();
    public DbSet<ContentAction> ContentActions => Set<ContentAction>();
    public DbSet<Quote> Quotes => Set<Quote>();
    #endregion

    #region Content Metadata
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Contributor> Contributors => Set<Contributor>();
    public DbSet<ContentTypeAction> ContentTypeActions => Set<ContentTypeAction>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<Entities.Action> Actions => Set<Entities.Action>();
    public DbSet<TonePool> TonePools => Set<TonePool>();
    public DbSet<Topic> Topics => Set<Topic>();
    #endregion

    #region Accounts
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<TimeTracking> TimeTrackings => Set<TimeTracking>();
    public DbSet<UserSource> UserSources => Set<UserSource>();
    public DbSet<UserMediaType> UserMediaTypes => Set<UserMediaType>();

    public DbSet<UserColleague> UserColleagues => Set<UserColleague>();
    #endregion

    #region Reports
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<FolderContent> FolderContents => Set<FolderContent>();
    public DbSet<Filter> Filters => Set<Filter>();
    public DbSet<ChartTemplate> ChartTemplates => Set<ChartTemplate>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<ReportTemplateChartTemplate> ReportTemplateChartTemplates => Set<ReportTemplateChartTemplate>();
    public DbSet<ReportSection> ReportSections => Set<ReportSection>();
    public DbSet<ReportSectionChartTemplate> ReportSectionChartTemplates => Set<ReportSectionChartTemplate>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportInstance> ReportInstances => Set<ReportInstance>();
    public DbSet<ReportInstanceContent> ReportInstanceContents => Set<ReportInstanceContent>();
    public DbSet<UserReport> UserReports => Set<UserReport>();

    public DbSet<AVOverviewSection> AVOverviewSections => Set<AVOverviewSection>();
    public DbSet<AVOverviewSectionItem> AVOverviewSectionItems => Set<AVOverviewSectionItem>();
    public DbSet<AVOverviewInstance> AVOverviewInstances => Set<AVOverviewInstance>();
    public DbSet<AVOverviewTemplate> AVOverviewTemplates => Set<AVOverviewTemplate>();
    public DbSet<AVOverviewTemplateSection> AVOverviewTemplateSections => Set<AVOverviewTemplateSection>();
    public DbSet<AVOverviewTemplateSectionItem> AVOverviewTemplateSectionItems => Set<AVOverviewTemplateSectionItem>();
    public DbSet<UserAVOverview> UserAVOverviews => Set<UserAVOverview>();

    public DbSet<EventSchedule> EventSchedules => Set<EventSchedule>();
    #endregion

    #region Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationInstance> NotificationInstances => Set<NotificationInstance>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<UserContentNotification> UserContentNotifications => Set<UserContentNotification>();
    #endregion

    #region Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UserProduct> UserProducts => Set<UserProduct>();
    #endregion

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
                    this.OnUpdate(entry, user);
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
        using var transaction = Database.BeginTransaction();
        try
        {
            var result = SaveChanges();
            transaction.Commit();
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var metadataName = entry.Metadata.Name;
                var dbValues = entry.GetDatabaseValues();
                var currentValues = entry.CurrentValues;
                var originalValues = entry.OriginalValues;
                var sb = new StringBuilder();

                foreach (var property in currentValues.Properties)
                {
                    var dbValue = dbValues?[property];
                    var currentValue = currentValues[property];
                    var originalValue = originalValues[property];

                    if (dbValue?.ToString() != originalValue?.ToString() ||
                        dbValue?.ToString() != currentValue?.ToString())
                    {
                        sb.Append($"[{property.Name} - Current: {currentValue}; DB: {dbValue}; Original: {originalValue}]");
                    }
                }

                _logger?.LogError("{metadataName}: {sb}", metadataName, sb);
            }
            throw;
        }
        catch (DbUpdateException)
        {
            transaction.Rollback();
            throw;
        }
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
