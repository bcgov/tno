using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Ingest;

/// <summary>
/// IngestModel class, provides a model that represents an source.
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
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - The product.
    /// </summary>
    public ProductModel? Product { get; set; }

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
    public ScheduleType ScheduleType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<IngestScheduleModel> Schedules { get; set; } = Array.Empty<IngestScheduleModel>();
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
        this.Topic = entity.Topic;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.ScheduleType = entity.ScheduleType;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
        this.RetryLimit = entity.RetryLimit;

        this.IngestTypeId = entity.IngestTypeId;
        this.IngestType = entity.IngestType != null ? new IngestTypeModel(entity.IngestType) : null;
        this.SourceId = entity.SourceId;
        this.Source = entity.Source != null ? new SourceModel(entity.Source) : null;
        this.ProductId = entity.ProductId;
        this.Product = entity.Product != null ? new ProductModel(entity.Product) : null;
        this.SourceConnectionId = entity.SourceConnectionId;
        this.SourceConnection = entity.SourceConnection != null ? new ConnectionModel(entity.SourceConnection, options) : null;
        this.DestinationConnectionId = entity.DestinationConnectionId;
        this.DestinationConnection = entity.DestinationConnection != null ? new ConnectionModel(entity.DestinationConnection, options) : null;

        this.LastRanOn = entity.State?.LastRanOn;
        this.FailedAttempts = entity.State?.FailedAttempts ?? 0;

        this.Schedules = entity.SchedulesManyToMany.Select(s => new IngestScheduleModel(s));
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
        entity.Configuration = JsonSerializer.Serialize(this.Configuration, options);
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Ingest(IngestModel model)
    {
        var entity = new Entities.Ingest(model.Name, model.Topic, model.SourceId, model.IngestTypeId, model.ProductId, model.SourceConnectionId, model.DestinationConnectionId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            ScheduleType = model.ScheduleType,
            Configuration = JsonSerializer.Serialize(model.Configuration),
            RetryLimit = model.RetryLimit,
            State = new Entities.IngestState(model.Id)
            {
                LastRanOn = model.LastRanOn,
                FailedAttempts = model.FailedAttempts,
            },
            Version = model.Version ?? 0
        };

        entity.SchedulesManyToMany.AddRange(model.Schedules.Select(s => s.ToEntity(entity.Id)));

        return entity;
    }
    #endregion
}
