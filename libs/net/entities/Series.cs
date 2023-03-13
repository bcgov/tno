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
    #endregion

    #region Constructors
    protected Series() { }

    public Series(string name) : base(name)
    {
    }
    #endregion
}
