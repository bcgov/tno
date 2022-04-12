using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_tone")]
public class ContentTonePool : AuditColumns, IEquatable<ContentTonePool>
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("tone_pool_id")]
    public int TonePoolId { get; set; }

    public virtual TonePool? TonePool { get; set; }

    [Column("value")]
    public int Value { get; set; }
    #endregion

    #region Constructors
    protected ContentTonePool() { }

    public ContentTonePool(Content content, TonePool tonePool, int value)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.TonePoolId = tonePool?.Id ?? throw new ArgumentNullException(nameof(tonePool));
        this.TonePool = tonePool;
        this.Value = value;
    }

    public ContentTonePool(long contentId, int tonePoolId, int value)
    {
        this.ContentId = contentId;
        this.TonePoolId = tonePoolId;
        this.Value = value;
    }
    #endregion

    #region Methods
    public bool Equals(ContentTonePool? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.TonePoolId == other.TonePoolId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentTonePool);
    public override int GetHashCode() => (this.ContentId, this.TonePoolId).GetHashCode();
    #endregion
}
