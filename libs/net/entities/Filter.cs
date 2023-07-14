using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// Filter class, provides a DB model to manage filters.
/// </summary>
[Table("filter")]
public class Filter : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this filter.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - The Elasticsearch query for this filter.
    /// </summary>
    [Column("query")]
    public JsonDocument Query { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The settings for this filter.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - List of report sections that use this filter.
    /// </summary>
    public virtual List<ReportSection> ReportSections { get; } = new List<ReportSection>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Filter object.
    /// </summary>
    protected Filter() : base() { }

    /// <summary>
    /// Creates a new instance of a Filter object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="owner"></param>
    /// <param name="template"></param>
    public Filter(string name, User owner)
        : this(0, name, owner?.Id)
    {
        this.Owner = owner;
    }

    /// <summary>
    /// Creates a new instance of a Filter object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="ownerId"></param>
    /// <param name="ownerId"></param>
    public Filter(int id, string name, int? ownerId) : base(id, name)
    {
        this.OwnerId = ownerId;
    }
    #endregion
}
