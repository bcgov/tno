using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_tone")]
public class ContentTonePool : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

    [Column("tone_pool_id")]
    public int TonePoolId { get; set; }

    public TonePool? TonePool { get; set; }

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

    public ContentTonePool(int contentId, int tonePoolId, int value)
    {
        this.ContentId = contentId;
        this.TonePoolId = tonePoolId;
        this.Value = value;
    }
    #endregion
}