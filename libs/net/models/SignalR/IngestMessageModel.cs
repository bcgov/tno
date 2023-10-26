namespace TNO.API.Models.SignalR;

/// <summary>
/// IngestMessageModel class, provides a model that represents an update to an ingest.
/// </summary>
public class IngestMessageModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The name of the ingest
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Whether the data source is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - Maximum number of failures before stopping.
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set - Number of sequential failures that have occurred.
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set - When the ingest service was ingested last.
    /// </summary>
    public DateTime? LastRanOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestMessageModel.
    /// </summary>
    public IngestMessageModel() { }

    /// <summary>
    /// Creates a new instance of an IngestMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public IngestMessageModel(Entities.Ingest entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.IsEnabled = entity.IsEnabled;
        this.RetryLimit = entity.RetryLimit;
        this.FailedAttempts = entity.State?.FailedAttempts ?? 0;
        this.LastRanOn = entity.State?.LastRanOn;
    }
    #endregion
}
