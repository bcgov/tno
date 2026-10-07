namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentEvidenceModel class, a content item's evidence document for report synthesis: its
/// analysis with the fields the source and media-type exclusions, approval, and publication rules
/// filter on. Documents are replaced or removed with their content.
/// </summary>
public class ContentEvidenceModel
{
    #region Properties
    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - The analysis.</summary>
    public long AnalysisId { get; set; }

    /// <summary>get/set - The source.</summary>
    public int? SourceId { get; set; }

    /// <summary>get/set - The media type.</summary>
    public int MediaTypeId { get; set; }

    /// <summary>get/set - Whether an audio/video transcript is approved; unapproved transcripts never reach subscribers.</summary>
    public bool IsApproved { get; set; }

    /// <summary>get/set - The content status.</summary>
    public string Status { get; set; } = "";

    /// <summary>get/set - Whether the content is published.</summary>
    public bool IsPublished { get; set; }

    /// <summary>get/set - The publication date.</summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>get/set - The label reports group by (staff topic, otherwise registry label).</summary>
    public string? Topic { get; set; }

    /// <summary>get/set - The headline.</summary>
    public string Headline { get; set; } = "";

    /// <summary>get/set - The summary.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - Key facts.</summary>
    public IEnumerable<string> Facts { get; set; } = Array.Empty<string>();

    /// <summary>get/set - People and organizations named.</summary>
    public IEnumerable<string> Entities { get; set; } = Array.Empty<string>();

    /// <summary>get/set - Verbatim quotes.</summary>
    public IEnumerable<AnalysisQuoteSummaryModel> Quotes { get; set; } = Array.Empty<AnalysisQuoteSummaryModel>();

    /// <summary>get/set - The content's projection revision; the document version.</summary>
    public long ProjectionRevision { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentEvidenceModel.
    /// </summary>
    public ContentEvidenceModel() { }

    /// <summary>
    /// Creates a new instance of a ContentEvidenceModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="analysis"></param>
    public ContentEvidenceModel(ContentModel content, ContentAnalysisSummaryModel analysis)
    {
        this.ContentId = content.Id;
        this.AnalysisId = analysis.Id;
        this.SourceId = content.SourceId;
        this.MediaTypeId = content.MediaTypeId;
        this.IsApproved = content.IsApproved || content.ContentType != TNO.Entities.ContentType.AudioVideo;
        this.Status = content.Status.ToString();
        this.IsPublished = content.Status == TNO.Entities.ContentStatus.Published || content.Status == TNO.Entities.ContentStatus.Publish;
        this.PublishedOn = content.PublishedOn;
        this.Topic = analysis.TopicLabel;
        this.Headline = content.Headline;
        this.Summary = analysis.Summary;
        this.Facts = analysis.KeyFacts.Select(f => f.Statement).ToArray();
        this.Entities = analysis.Entities.Select(e => e.Name).ToArray();
        this.Quotes = analysis.Quotes.ToArray();
        this.ProjectionRevision = content.ProjectionRevision;
    }
    #endregion
}
