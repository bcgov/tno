using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// ContentType class, provides a way to identify the content type and the form it will use.
/// </summary>
[Cache("content_types", "lookups")]
[Table("content_type")]
public class ContentType : BaseType<int>
{
    #region Properties
    public virtual List<DataSource> DataSources { get; } = new List<DataSource>();
    public virtual List<Content> Contents { get; } = new List<Content>();
    public virtual List<Action> Actions { get; } = new List<Action>();
    public virtual List<ContentTypeAction> ActionsManyToMany { get; } = new List<ContentTypeAction>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentType object.
    /// </summary>
    protected ContentType() { }

    /// <summary>
    /// Creates a new instance of a ContentType object, initializes with specified parameter.
    /// </summary>
    /// <param name="name"></param>
    public ContentType(string name) : base(name)
    {
    }
    #endregion
}
