using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// Folder class, provides a DB model to manage folders.
/// </summary>
[Table("folder")]
public class Folder : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this folder.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - The settings for this folder.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - List of content in this folder (many-to-many).
    /// </summary>
    public virtual List<FolderContent> ContentManyToMany { get; } = new List<FolderContent>();

    /// <summary>
    /// get - List of content in this folder.
    /// </summary>
    public virtual List<Content> Content { get; } = new List<Content>();

    /// <summary>
    /// get - List of report sections that use this folder.
    /// </summary>
    public virtual List<ReportSection> ReportSections { get; } = new List<ReportSection>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Folder object.
    /// </summary>
    protected Folder() : base() { }

    /// <summary>
    /// Creates a new instance of a Folder object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="owner"></param>
    /// <param name="template"></param>
    public Folder(string name, User owner)
        : this(0, name, owner?.Id)
    {
        this.Owner = owner;
    }

    /// <summary>
    /// Creates a new instance of a Folder object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="ownerId"></param>
    /// <param name="ownerId"></param>
    public Folder(int id, string name, int? ownerId) : base(id, name)
    {
        this.OwnerId = ownerId;
    }
    #endregion
}
