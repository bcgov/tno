using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentFieldOwnership class, provides a DB model recording who owns a populated editorial value,
/// or that a person cleared it. A value with no record is human-owned (every value that existed
/// before ownership was recorded, and every value a person or ingest set).
/// Scalar fields (summary, contributor) use an empty value key; collection values (tags, quotes,
/// topics) use the item's key.
/// </summary>
[Table("content_field_ownership")]
public class ContentFieldOwnership : AuditColumns
{
    #region Variables
    /// <summary>The summary field.</summary>
    public const string SummaryField = "summary";
    /// <summary>The contributor field.</summary>
    public const string ContributorField = "contributor";
    /// <summary>A tag, keyed by tag ID.</summary>
    public const string TagField = "tag";
    /// <summary>A quote, keyed by quote ID.</summary>
    public const string QuoteField = "quote";
    /// <summary>A topic, keyed by topic ID.</summary>
    public const string TopicField = "topic";
    #endregion

    #region Properties
    /// <summary>get/set - Foreign key to the content.</summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>get/set - The content.</summary>
    public virtual Content? Content { get; set; }

    /// <summary>get/set - The field (summary, contributor, tag, quote, topic).</summary>
    [Column("field")]
    public string Field { get; set; } = "";

    /// <summary>get/set - The collection item's key, or empty for a scalar field.</summary>
    [Column("value_key")]
    public string ValueKey { get; set; } = "";

    /// <summary>get/set - Who owns the value.</summary>
    [Column("owner")]
    public FieldOwner Owner { get; set; }

    /// <summary>get/set - A person removed the value, so analysis never fills it again.</summary>
    [Column("is_cleared")]
    public bool IsCleared { get; set; }

    /// <summary>get/set - The analysis that populated the value.</summary>
    [Column("analysis_id")]
    public long? AnalysisId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentFieldOwnership object.
    /// </summary>
    protected ContentFieldOwnership() { }

    /// <summary>
    /// Creates a new instance of a ContentFieldOwnership object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="field"></param>
    /// <param name="valueKey"></param>
    /// <param name="owner"></param>
    public ContentFieldOwnership(long contentId, string field, string valueKey, FieldOwner owner)
    {
        this.ContentId = contentId;
        this.Field = field;
        this.ValueKey = valueKey;
        this.Owner = owner;
    }
    #endregion
}
