using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;
using TNO.Entities.Validation;

namespace TNO.Entities;

/// <summary>
/// Ingest class, provides an entity model which enabled configuring different ingestion services.
/// An ingestion service fetches content for a specified source, source connection, and ingest type.
/// It will the provide a way to import content to the specified media type and destination connection.
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
    /// get/set - Foreign key to media type.
    /// </summary>
    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The media type.
    /// </summary>
    public virtual MediaType? MediaType { get; set; }

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
    public JsonDocument Configuration { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Maximum number of failures before stopping.
    /// </summary>
    [Column("retry_limit")]
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set - Failure limit can be reset after this many seconds wait since LastRun.
    /// Default value of 0 means never auto reset.
    /// </summary>
    [Column("reset_retry_after_delay_ms")]
    public int ResetRetryAfterDelayMs { get; set; } = 0;

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
    /// <summary>
    /// Creates a new instance of an Ingest object.
    /// </summary>
    protected Ingest() { }

    /// <summary>
    /// Creates a new instance of an Ingest object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="topic"></param>
    /// <param name="sourceId"></param>
    /// <param name="ingestTypeId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="sourceConnectionId"></param>
    /// <param name="destinationConnectionId"></param>
    /// <param name="scheduleType"></param>
    public Ingest(string name, string topic, int sourceId, int ingestTypeId, int mediaTypeId, int sourceConnectionId, int destinationConnectionId, ScheduleType scheduleType)
        : this(name, topic, sourceId, ingestTypeId, mediaTypeId, sourceConnectionId, destinationConnectionId)
    {
        this.ScheduleType = scheduleType;
    }

    /// <summary>
    /// Creates a new instance of an Ingest object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="topic"></param>
    /// <param name="sourceId"></param>
    /// <param name="ingestTypeId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="sourceConnectionId"></param>
    /// <param name="destinationConnectionId"></param>
    /// <exception cref="ArgumentException"></exception>
    public Ingest(string name, string topic, int sourceId, int ingestTypeId, int mediaTypeId, int sourceConnectionId, int destinationConnectionId)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(topic));

        this.Name = name;
        this.Topic = topic;
        this.SourceId = sourceId;
        this.IngestTypeId = ingestTypeId;
        this.MediaTypeId = mediaTypeId;
        this.SourceConnectionId = sourceConnectionId;
        this.DestinationConnectionId = destinationConnectionId;
        this.ScheduleType = ScheduleType.None;
        this.RetryLimit = 3;
    }

    /// <summary>
    /// Creates a new instance of an Ingest object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source"></param>
    /// <param name="ingestType"></param>
    /// <param name="mediaType"></param>
    /// <param name="sourceConnection"></param>
    /// <param name="destinationConnection"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Ingest(string name, Source source, IngestType ingestType, MediaType mediaType, Connection sourceConnection, Connection destinationConnection)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.Topic = source?.Code ?? throw new ArgumentNullException(nameof(source));
        this.IngestTypeId = ingestType?.Id ?? throw new ArgumentNullException(nameof(ingestType));
        this.IngestType = ingestType;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
        this.SourceConnectionId = sourceConnection?.Id ?? throw new ArgumentNullException(nameof(sourceConnection));
        this.SourceConnection = sourceConnection;
        this.DestinationConnectionId = destinationConnection?.Id ?? throw new ArgumentNullException(nameof(destinationConnection));
        this.DestinationConnection = destinationConnection;
        this.ScheduleType = ScheduleType.None;
        this.RetryLimit = 3;
    }
    #endregion
}
