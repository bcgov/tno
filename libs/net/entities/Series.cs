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
    /// get - List of content linked to this series.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of topic score rules linked to this series.
    /// </summary>
    public virtual List<TopicScoreRule> ScoreRules { get; } = new List<TopicScoreRule>();
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
    #endregion
}
