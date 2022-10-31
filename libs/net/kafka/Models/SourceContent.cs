using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// TODO: Change name to SourceContentModel for consistent naming convention
/// </summary>
public class SourceContent : ContentBase
{
    #region Properties
    /// <summary>
    /// get/set - The story body
    /// </summary>
    public string Body { get; set; } = "";
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
    /// <param name="productId"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <param name="body"></param>
    /// <param name="publishedOn"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SourceContent(string dataLocation, string source, ContentType contentType, int productId, string uid, string title, string summary, string body, DateTime publishedOn)
        : base(dataLocation, source, contentType, productId, uid, title, summary, publishedOn)
    {
        this.Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    #endregion
}
