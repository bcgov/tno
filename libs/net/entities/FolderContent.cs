using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// FolderContent class, provides a DB model to associate content with a folder.
/// </summary>
[Table("folder_content")]
public class FolderContent : AuditColumns, IEquatable<FolderContent>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the folder.
    /// </summary>
    [Key]
    [Column("folder_id")]
    public int FolderId { get; set; }

    /// <summary>
    /// get/set - The folder.
    /// </summary>
    public Folder? Folder { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the content.
    /// </summary>
    [Key]
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content for this report.
    /// </summary>
    public Content? Content { get; set; }

    /// <summary>
    /// get/set - The order the content is returned.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderContent object.
    /// </summary>
    protected FolderContent() { }

    /// <summary>
    /// Creates a new instance of a FolderContent object, initializes with specified parameters.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="content"></param>
    /// <param name="sortOrder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FolderContent(Folder folder, Content content, int sortOrder = 0)
    {
        this.Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        this.FolderId = folder.Id;
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a FolderContent object, initializes with specified parameters.
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="contentId"></param>
    /// <param name="sortOrder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FolderContent(int folderId, long contentId, int sortOrder = 0)
    {
        this.FolderId = folderId;
        this.ContentId = contentId;
        this.SortOrder = sortOrder;
    }
    #endregion

    #region Methods
    public bool Equals(FolderContent? other)
    {
        if (other == null) return false;
        return this.FolderId == other.FolderId
            && this.ContentId == other.ContentId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentAction);
    public override int GetHashCode() => (this.FolderId, this.ContentId).GetHashCode();
    #endregion
}
