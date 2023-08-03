namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// FolderContentModel class, provides a model that represents a content item in a folder.
/// </summary>
public class FolderContentModel : TNO.API.Areas.Services.Models.Content.ContentModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the parent folder.
    /// </summary>
    public int FolderId { get; set; }

    /// <summary>
    /// get/set - Sort order of content.
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
    public FolderContentModel(Entities.FolderContent entity) : base(entity.Content ?? throw new ArgumentNullException(nameof(entity)))
    {
        this.FolderId = entity.FolderId;
        this.SortOrder = entity.SortOrder;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.FolderContent(FolderContentModel model)
    {
        var entity = new Entities.FolderContent(model.FolderId, model.Id, model.SortOrder)
        {
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
