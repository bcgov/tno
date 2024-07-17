using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// UsersTones class, provides an entity to store News Item Tone records in the database.
/// </summary>
[Table("USERS_TONES")]
public class UserTone
{
    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ITEM_RSN")]
    public long ItemRSN { get; set; }

    /// <summary>
    /// get/set - The parent news item.
    /// </summary>
    public NewsItem? NewsItem { get; set; }

    /// <summary>
    /// get/set - The parent news item.
    /// </summary>
    public HNewsItem? HNewsItem { get; set; }

    /// <summary>
    /// get/set - The parent news item.
    /// </summary>
    public AllNewsItem? AllNewsItem { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("USER_RSN")]
    public long UserRSN { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TONE")]
    public long ToneValue { get; set; }
}
