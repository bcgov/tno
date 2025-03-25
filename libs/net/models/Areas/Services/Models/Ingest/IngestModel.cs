using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.Ingest;

/// <summary>
/// IngestModel class, provides a model that represents an ingest configuration.
/// </summary>
public class IngestModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int SourceConnectionId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ConnectionModel? SourceConnection { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int DestinationConnectionId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ConnectionModel? DestinationConnection { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public SourceModel? Source { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int IngestTypeId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IngestTypeModel? IngestType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public MediaTypeModel? MediaType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ScheduleType ScheduleType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set - Creation date of last ingested content item.
    /// </summary>
    public DateTime? CreationDateOfLastItem { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set - Number of milliseconds to wait before retrying after a series of failures.
    /// This should auto restart a failed ingestion service.
    /// </summary>
    public int ResetRetryAfterDelayMs { get; set; } = 300000;

    /// <summary>
    /// get/set -
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<IngestScheduleModel> IngestSchedules { get; set; } = Array.Empty<IngestScheduleModel>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<DataLocationModel> DataLocations { get; set; } = Array.Empty<DataLocationModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestModel.
    /// </summary>
    public IngestModel() { }

    /// <summary>
    /// Creates a new instance of an IngestModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public IngestModel(Entities.Ingest entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Topic = entity.Topic;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.SourceConnectionId = entity.SourceConnectionId;
        this.SourceConnection = entity.SourceConnection != null ? new ConnectionModel(entity.SourceConnection, options) : null;
        this.DestinationConnectionId = entity.DestinationConnectionId;
        this.DestinationConnection = entity.DestinationConnection != null ? new ConnectionModel(entity.DestinationConnection, options) : null;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType != null ? new MediaTypeModel(entity.MediaType) : null;
        this.IngestTypeId = entity.IngestTypeId;
        this.IngestType = entity.IngestType != null ? new IngestTypeModel(entity.IngestType) : null;
        this.SourceId = entity.SourceId;
        this.Source = entity.Source != null ? new SourceModel(entity.Source, options) : null;
        this.RetryLimit = entity.RetryLimit;
        this.ResetRetryAfterDelayMs = entity.ResetRetryAfterDelayMs;
        this.ScheduleType = entity.ScheduleType;

        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();

        this.LastRanOn = entity.State?.LastRanOn;
        this.FailedAttempts = entity.State?.FailedAttempts ?? 0;
        this.CreationDateOfLastItem = entity.State?.CreationDateOfLastItem;

        this.IngestSchedules = entity.SchedulesManyToMany.Select(s => new IngestScheduleModel(s));
        this.DataLocations = entity.DataLocationsManyToMany.Where(d => d.DataLocation != null).Select(d => new DataLocationModel(d.DataLocation!, options));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Ingest object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.Ingest ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Ingest)this;
        entity.Configuration = JsonDocument.Parse(JsonSerializer.Serialize(this.Configuration, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Ingest(IngestModel model)
    {
        var entity = new Entities.Ingest(model.Name, model.Topic, model.SourceId, model.IngestTypeId, model.MediaTypeId, model.SourceConnectionId, model.DestinationConnectionId, model.ScheduleType)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Configuration = JsonDocument.Parse(JsonSerializer.Serialize(model.Configuration)),
            RetryLimit = model.RetryLimit,
            State = new Entities.IngestState(model.Id)
            {
                LastRanOn = model.LastRanOn,
                FailedAttempts = model.FailedAttempts,
                CreationDateOfLastItem = model.CreationDateOfLastItem
            },
            Version = model.Version ?? 0
        };

        if (model.IngestSchedules.Any())
            entity.Schedules.AddRange(model.IngestSchedules.Where(ds => ds.Schedule != null).Select(ds => (Entities.Schedule)ds));

        return entity;
    }
    #endregion
}
