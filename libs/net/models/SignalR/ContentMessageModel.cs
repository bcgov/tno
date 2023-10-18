using TNO.Entities;

namespace TNO.API.Models.SignalR;

/// <summary>
/// ContentMessageModel class, provides a model that represents a signalR clip message.
/// </summary>
public class ContentMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The type of content.
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - The status of the content.
    /// </summary>
    public ContentStatus Status { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user requested this content.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Content headline.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - Version number.
    /// </summary>
    public long? Version { get; set; }

    /// <summary>
    /// get/set - The reason the message has been sent.
    /// The app can then act based on the reason.
    /// </summary>
    public string? Reason { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentMessageModel.
    /// </summary>
    public ContentMessageModel() { }

    /// <summary>
    /// Creates a new instance of an ContentMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="reason"></param>
    public ContentMessageModel(Entities.Content entity, string? reason = null)
    {
        this.Id = entity.Id;
        this.ContentType = entity.ContentType;
        this.Status = entity.Status;
        this.OwnerId = entity.OwnerId;
        this.Headline = entity.Headline;
        this.Version = entity.Version;
        this.Reason = reason;
    }

    /// <summary>
    /// Creates a new instance of an ContentMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="reason"></param>
    public ContentMessageModel(TNO.API.Areas.Services.Models.Content.ContentModel model, string? reason = null)
    {
        this.Id = model.Id;
        this.ContentType = model.ContentType;
        this.Status = model.Status;
        this.OwnerId = model.OwnerId;
        this.Headline = model.Headline;
        this.Version = model.Version;
        this.Reason = reason;
    }
    #endregion
}
