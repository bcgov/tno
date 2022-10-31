using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;
using TNO.Entities.Validation;

namespace TNO.Entities;

/// <summary>
/// Ingest class, provides an entity model which enabled configuring different ingestion services.
/// An ingestion service fetches content for a specified source, source connection, and ingest type.
/// It will the provide a way to import content to the specified product and destination connection.
/// </summary>
[Cache("ingests", "lookups")]
[Table("ingest")]
public class Ingest : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify data source.  Identity insert value.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - Full name of data source.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic this data source is ingested to.
    /// </summary>
    [Column("topic")]
    [KafkaTopic]
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set - Long description of data source.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the data source is enabled.
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public virtual Source? Source { get; set; }

    /// <summary>
    /// get/set - Foreign key to ingest type.
    /// </summary>
    [Column("ingest_type_id")]
    public int IngestTypeId { get; set; }

    /// <summary>
    /// get/set - The ingest type.
    /// </summary>
    public virtual IngestType? IngestType { get; set; }

    /// <summary>
    /// get/set - Foreign key to product.
    /// </summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - The product.
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source connection.
    /// </summary>
    [Column("source_connection_id")]
    public int SourceConnectionId { get; set; }

    /// <summary>
    /// get/set - The source connection.
    /// </summary>
    public virtual Connection? SourceConnection { get; set; }

    /// <summary>
    /// get/set - Foreign key to the destination connection.
    /// </summary>
    [Column("destination_connection_id")]
    public int DestinationConnectionId { get; set; }

    /// <summary>
    /// get/set - The destination connection.
    /// </summary>
    public virtual Connection? DestinationConnection { get; set; }

    /// <summary>
    /// get/set - Configuration settings.
    /// </summary>
    [Column("configuration")]
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// get/set - Maximum number of attempts after a failure.
    /// </summary>
    [Column("retry_limit")]
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set - Ingest service state.
    /// </summary>
    public virtual IngestState? State { get; set; }

    /// <summary>
    /// get/set - The type of schedule this data source ingestion service will use.
    /// </summary>
    [Column("schedule_type")]
    public ScheduleType ScheduleType { get; set; }

    /// <summary>
    /// get - List of schedule linked to this ingest.
    /// </summary>
    public virtual List<Schedule> Schedules { get; } = new List<Schedule>();

    /// <summary>
    /// get - List of schedule (many-to-many) linked to this ingest.
    /// </summary>
    public virtual List<IngestSchedule> SchedulesManyToMany { get; } = new List<IngestSchedule>();

    /// <summary>
    /// get - Collection of data locations associated with this ingest.
    /// </summary>
    public virtual List<DataLocation> DataLocations { get; } = new List<DataLocation>();

    /// <summary>
    /// get - Collection of data locations, the many-to-many relationship.
    /// </summary>
    public virtual List<IngestDataLocation> DataLocationsManyToMany { get; } = new List<IngestDataLocation>();
    #endregion

    #region Constructors
    protected Ingest() { }

    public Ingest(string name, Source source, IngestType ingestType, Product product, Connection sourceConnection, Connection destinationConnection)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.Topic = source?.Code ?? throw new ArgumentNullException(nameof(source));
        this.IngestTypeId = ingestType?.Id ?? throw new ArgumentNullException(nameof(ingestType));
        this.IngestType = ingestType;
        this.ProductId = product?.Id ?? throw new ArgumentNullException(nameof(product));
        this.Product = product;
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
        this.SourceConnectionId = sourceConnection?.Id ?? throw new ArgumentNullException(nameof(sourceConnection));
        this.SourceConnection = sourceConnection;
        this.DestinationConnectionId = destinationConnection?.Id ?? throw new ArgumentNullException(nameof(destinationConnection));
        this.DestinationConnection = destinationConnection;
        this.ScheduleType = ScheduleType.None;
    }

    public Ingest(string name, string topic, int sourceId, int ingestTypeId, int productId, int sourceConnectionId, int destinationConnectionId)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(topic));

        this.Name = name;
        this.Topic = topic;
        this.SourceId = sourceId;
        this.IngestTypeId = ingestTypeId;
        this.ProductId = productId;
        this.SourceConnectionId = sourceConnectionId;
        this.DestinationConnectionId = destinationConnectionId;
        this.ScheduleType = ScheduleType.None;
    }

    public Ingest(string name, string topic, int sourceId, int ingestTypeId, int productId, int sourceConnectionId, int destinationConnectionId, ScheduleType scheduleType)
        : this(name, topic, sourceId, ingestTypeId, productId, sourceConnectionId, destinationConnectionId)
    {
        this.ScheduleType = scheduleType;
    }
    #endregion
}
