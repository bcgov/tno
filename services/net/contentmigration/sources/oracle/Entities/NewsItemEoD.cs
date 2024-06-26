using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// NewsItemEoD class, provides an entity to store News Item EoD records in the database.
/// </summary>
[Table("NEWS_ITEMS_EOD")]
public class NewsItemEoD
{
    #region Properties
    /// <summary>
    /// get/set.
    /// </summary>
    [Key]
    [Column("ITEM_RSN")]
    public long ItemRSN { get; set; }

    /// <summary>
    /// get/set - The parent new item.
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
    [Column("EOD_CATEGORY", TypeName = "VARCHAR2")]
    public string? Category { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_CATEGORY_GROUP", TypeName = "VARCHAR2")]
    public string? CategoryGroup { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_DATE", TypeName = "VARCHAR2")]
    public string? EoDDate { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ITEM_SCORE")]
    public int Score { get; set; }
    #endregion
}
