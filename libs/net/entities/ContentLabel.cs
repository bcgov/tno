using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentLabel class, provides an entity model that represents a label that provides details about content (i.e. keyword, person, location, ...)
/// </summary>
[Table("content_label")]
public class ContentLabel : AuditColumns, IEquatable<ContentLabel>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity seed.
    /// </summary>
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The parent content.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - The label key to group related labels.
    /// </summary>
    [Column("key")]
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The value of the label.
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentLabel object.
    /// </summary>
    protected ContentLabel() { }

    /// <summary>
    /// Creates a new instance of a ContentLabel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentLabel(Content content, string key, string value)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.Key = key ?? throw new ArgumentNullException(nameof(key));
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new instance of a ContentLabel object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentLabel(long contentId, string key, string value)
    {
        this.ContentId = contentId;
        this.Key = key ?? throw new ArgumentNullException(nameof(key));
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determines if the objects are equal.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ContentLabel? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.Key == other.Key && this.Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentLabel);
    public override int GetHashCode() => (this.ContentId, this.Key, this.Value).GetHashCode();
    #endregion
}
