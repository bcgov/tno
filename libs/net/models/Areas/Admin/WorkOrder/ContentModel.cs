namespace TNO.API.Areas.Admin.Models.WorkOrder;

/// <summary>
/// ContentModel class, provides a model that represents an user.
/// </summary>
public class ContentModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public long Id { get; set; } = default!;

    /// <summary>
    /// get/set - headline of content.
    /// </summary>
    public string Headline { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentModel.
    /// </summary>
    public ContentModel() { }

    /// <summary>
    /// Creates a new instance of an ContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentModel(Entities.Content entity)
    {
        this.Id = entity.Id;
        this.Headline = entity.Headline;
    }
    #endregion
}
