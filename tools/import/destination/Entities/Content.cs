using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content")]
public class Content : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("status")]
    public ContentStatus Status { get; set; }

    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; }

    [Column("content_type_id")]
    public int ContentTypeId { get; set; }

    public ContentType? ContentType { get; set; }

    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    public MediaType? MediaType { get; set; }

    [Column("license_id")]
    public int LicenseId { get; set; }

    public License? License { get; set; }

    [Column("series_id")]
    public int? SeriesId { get; set; }

    public Series? Series { get; set; }

    [Column("owner_id")]
    public int OwnerId { get; set; }

    public User? Owner { get; set; }

    [Column("data_source_id")]
    public int? DataSourceId { get; set; }

    public DataSource? DataSource { get; set; }

    [Column("source")]
    public string Source { get; set; } = "";

    [Column("headline")]
    public string Headline { get; set; } = "";

    [Column("uid")]
    public string Uid { get; set; } = "";

    [Column("page")]
    public string Page { get; set; } = "";

    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    [Column("summary")]
    public string Summary { get; set; } = "";

    [Column("transcription")]
    public string Transcription { get; set; } = "";

    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    public List<ContentCategory> Categories { get; set; } = new List<ContentCategory>();

    public List<ContentTonePool> TonePools { get; set; } = new List<ContentTonePool>();

    public List<ContentAction> Actions { get; set; } = new List<ContentAction>();

    public List<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();

    public List<Tag> Tags { get; set; } = new List<Tag>();

    public List<FileReference> FileReferences { get; set; } = new List<FileReference>();

    public List<ContentLink> Links { get; set; } = new List<ContentLink>();
    #endregion

    #region Constructors
    protected Content() { }

    public Content(string uid, string headline, string source)
    {
        if (String.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(uid));
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));

        this.Uid = uid;
        this.Headline = headline;
        this.Source = source;
    }

    public Content(string uid, string headline, DataSource source)
    {
        if (String.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(uid));
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));

        this.Uid = uid;
        this.Headline = headline;
        this.DataSourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.DataSource = source;
        this.Source = source.Name;
    }
    #endregion
}