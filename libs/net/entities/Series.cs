using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Series class, provides an entity model to group related content by series, byline (author), program name.
/// </summary>
[Cache("series", "lookups")]
[Table("series")]
public class Series : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source this series belongs to.
    /// </summary>
    public Source? Source { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether to show topics on the content form.
    /// </summary>
    [Column("use_in_topics")]
    public bool UseInTopics { get; set; }

    /// <summary>
    /// get/set - is CBRA source or not.
    /// </summary>
    [Column("is_cbra_source")]
    public bool IsCBRASource { get; set; }

    /// <summary>
    /// get/set - Is a secondary source - generally added via use of "Other" field.
    /// Will not be displayed in the primary Series/Source dropdown or in search filters
    /// </summary>
    [Column("is_other")]
    public bool IsOther { get; set; }

    /// <summary>
    /// get - List of content linked to this series.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of topic score rules linked to this series.
    /// </summary>
    public virtual List<TopicScoreRule> ScoreRules { get; } = new List<TopicScoreRule>();

    /// <summary>
    /// get - Collection of media types - used in search mapping.
    /// </summary>
    public virtual List<MediaType> MediaTypeSearchMappings { get; } = new List<MediaType>();

    /// <summary>
    /// get - Collection of media types used in search mapping, the many-to-many relationship.
    /// </summary>
    public virtual List<SeriesMediaTypeSearchMapping> MediaTypeSearchMappingsManyToMany { get; } = new List<SeriesMediaTypeSearchMapping>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Series object.
    /// </summary>
    protected Series() { }

    /// <summary>
    /// Creates a new instance of a Series object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source"></param>
    public Series(string name, Source source) : base(name)
    {
        this.Source = source;
        this.SourceId = source?.Id;
    }

    /// <summary>
    /// Creates a new instance of a Series object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourceId"></param>
    public Series(string name, int? sourceId = null) : base(name)
    {
        this.SourceId = sourceId;
    }
    
    /// <summary>
    /// Creates a new instance of a Series object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isOther"></param>
    /// <param name="sourceId"></param>
    public Series(string name, bool isOther, int? sourceId = null) : this(name, sourceId)
    {
        this.IsOther = isOther;
    }
    #endregion
}
