namespace TNO.API.Models.SignalR;

/// <summary>
/// ContentActionMessageModel class, provides a model that represents a signalR clip message.
/// </summary>
public class ContentActionMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Primary key to action.
    /// </summary>
    public int ActionId { get; set; }

    /// <summary>
    /// get/set - Content action value.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// get/set - Version number.
    /// </summary>
    public long? Version { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentActionMessageModel.
    /// </summary>
    public ContentActionMessageModel() { }

    /// <summary>
    /// Creates a new instance of an ContentActionMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentActionMessageModel(Entities.ContentAction entity)
    {
        this.ContentId = entity.ContentId;
        this.ActionId = entity.ActionId;
        this.Value = entity.Value;
        this.Version = entity.Version;
    }
    #endregion
}
