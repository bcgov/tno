using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Contributor class, provides an entity model to group related content by columnist or pundit.
/// </summary>
[Cache("contributor", "lookups")]
[Table("contributor")]
public class Contributor : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source this contributor belongs to.
    /// </summary>
    public Source? Source { get; set; }

    /// <summary>
    /// get/set - Whether content with this contributor should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get - List of content linked to this contributor.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Contributor object.
    /// </summary>
    protected Contributor() { }

    /// <summary>
    /// Creates a new instance of a Contributor object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source"></param>
    public Contributor(string name, Source source) : base(name)
    {
        this.Source = source;
        this.SourceId = source?.Id;
    }

    /// <summary>
    /// Creates a new instance of a Contributor object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourceId"></param>
    public Contributor(string name, int? sourceId = null) : base(name)
    {
        this.SourceId = sourceId;
    }
    #endregion
}
