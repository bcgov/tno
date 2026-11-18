using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// TODO: Change name to SourceContentModel for consistent naming convention
/// </summary>
public class SourceContent : ContentBase
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the user who requested the content to be created.
    /// </summary>
    public int? RequestedById { get; set; }

    /// <summary>
    /// get/set - A unique key to identify this content - comes from external source.
    /// </summary>
    public string ExternalUid { get; set; } = "";

    /// <summary>
    /// get/set - A unique key to identify this content - comes from internal source.
    /// </summary>
    public string HashUid { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SourceContent object.
    /// </summary>
    public SourceContent() { }

    /// <summary>
    /// Creates a new instance of a SourceContent object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataLocation"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <param name="body"></param>
    /// <param name="publishedOn"></param>
    /// <param name="publish"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SourceContent(string dataLocation, string source, ContentType contentType, int mediaTypeId,
        string uid, string title, string summary, string body, DateTime? publishedOn, bool publish = false)
        : base(dataLocation, source, contentType, mediaTypeId, uid, title, summary, publishedOn)
    {
        this.Body = body ?? throw new ArgumentNullException(nameof(body));
        if (publish) Status = ContentStatus.Publish;
    }
    #endregion
}
