using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// TODO: Change name to SourceContentModel for consistent naming convention
/// </summary>
public class SourceContent : ContentBase
{
    #region Properties
    public string Body { get; set; } = "";
    #endregion

    #region Constructors
    public SourceContent() { }

    public SourceContent(string source, ContentType contentType, int productId, string uid, string title, string summary, string body, DateTime publishedOn)
        : base(source, contentType, productId, uid, title, summary, publishedOn)
    {
        this.Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    public SourceContent(string source, ContentType contentType, int productId, int? connectionId, string uid, string title, string summary, string body, DateTime publishedOn)
        : base(source, contentType, productId, connectionId, uid, title, summary, publishedOn)
    {
        this.Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    #endregion
}
