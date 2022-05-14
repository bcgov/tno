namespace TNO.Models.Kafka;

public class SourceContent : ContentBase
{
    #region Properties
    public string Body { get; set; } = "";
    #endregion

    #region Constructors
    public SourceContent() { }

    public SourceContent(SourceMediaType mediaType, string source, string uid, string title, string summary, string body, DateTime publishedOn) : base(mediaType, source, uid, title, summary, publishedOn)
    {
        this.Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    #endregion
}
