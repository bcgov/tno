namespace TNO.API.Areas.Report;

/// <summary>
/// ContentTonePoolModel class, provides a model that represents an tone pool.
/// </summary>
public class ContentTonePoolModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The user Id who owns this tone pool.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - The value of the tone pool.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// get/set - Whether this tone pool is public.
    /// </summary>
    public bool IsPublic { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel.
    /// </summary>
    public ContentTonePoolModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTonePoolModel(Entities.ContentTonePool entity)
    {
        this.Id = entity.TonePoolId;
        this.Name = entity.TonePool?.Name ?? "";
        this.OwnerId = entity.TonePool?.OwnerId ?? 0;
        this.Value = entity.Value;
        this.IsPublic = entity.TonePool?.IsPublic ?? false;
    }

    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTonePoolModel(TNO.API.Areas.Services.Models.Content.ContentTonePoolModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.OwnerId = model.OwnerId;
        this.Value = model.Value;
        this.IsPublic = model.IsPublic;
    }
    #endregion
}
