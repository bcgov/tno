namespace TNO.API.Areas.Admin.Models.Folder;

/// <summary>
/// FolderContentModel class, provides a model that represents an user.
/// </summary>
public class FolderContentModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Unique username to identify user.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FolderContentModel.
    /// </summary>
    public FolderContentModel() { }

    /// <summary>
    /// Creates a new instance of an FolderContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public FolderContentModel(Entities.FolderContent entity)
    {
        this.ContentId = entity.ContentId;
        this.SortOrder = entity.SortOrder;
    }
    #endregion
}
