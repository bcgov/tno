namespace TNO.API.Areas.Editor.Models.Folder;

/// <summary>
/// FolderContentModel class, provides a model that represents the folder content.
/// </summary>
public class FolderContentModel
{
    #region Properties
    /// <summary>
    /// get/set -  The content id.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The sort order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The maximum score this piece of content can be assigned.
    /// </summary>
    public int? MaxTopicScore { get; set; }

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public ContentModel? Content { get; set; }
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
        this.Content = entity.Content != null ? new ContentModel(entity.Content) : null;
    }
    #endregion
}
