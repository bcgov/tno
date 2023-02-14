using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// IngestType class, provides an entity model to identify the type of content, and the form to display.
/// </summary>
[Cache("ingest_types", "lookups")]
[Table("ingest_type")]
public class IngestType : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Identifies the type of content and the form to use.
    /// </summary>
    [Column("content_type")]
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should not allow transcriptions.
    /// </summary>
    [Column("disable_transcribe")]
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get - List of ingests linked to this ingest type.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of content linked to this ingest type.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    protected IngestType() { }

    public IngestType(string name, ContentType contentType = ContentType.Snippet) : base(name)
    {
        this.ContentType = contentType;
    }
    #endregion
}
