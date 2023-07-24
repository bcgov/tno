using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplate class, provides a DB model to manage different types of overviews.
/// </summary>
[Table("av_overview_section_item")]
public class AVOverviewSectionItem : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the overview template.
    /// </summary>
    [Column("av_overview_section_id")]
    public int AVOverviewSectionId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("item_type")]
    public AVOverviewItemType ItemType { get; set; }

    /// <summary>
    /// get/set - The source time of the item.
    /// </summary>
    [Column("time")]
    public string Time { get; set; } = "";

    /// <summary>
    /// get/set - Story abstract or summary text.
    /// </summary>
    [Column("summary")]
    public string Summary { get; set; } = "";

    /// <summary>
    /// get - Foreign key to the owning content.
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }


    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a av overview template object.
    /// </summary>
    protected AVOverviewSectionItem() : base() { }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    public AVOverviewSectionItem(string name) : base(name)
    {

    }
    #endregion
}
