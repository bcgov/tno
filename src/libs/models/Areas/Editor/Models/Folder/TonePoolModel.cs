namespace TNO.API.Areas.Editor.Models.Folder;

/// <summary>
/// TonePoolModel class, provides a model that represents an tone pool.
/// </summary>
public class TonePoolModel
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TonePoolModel.
    /// </summary>
    public TonePoolModel() { }

    /// <summary>
    /// Creates a new instance of an TonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TonePoolModel(Entities.ContentTonePool entity)
    {
        this.Id = entity.TonePoolId;
        this.Name = entity.TonePool?.Name ?? "";
        this.OwnerId = entity.TonePool?.OwnerId ?? 0;
        this.Value = entity.Value;
    }
    #endregion
}
