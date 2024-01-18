using TNO.Entities;

namespace TNO.API.Areas.Services.Models.AVOverview;

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
    /// get/set - The type of content and form to use.
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - An array of file references.
    /// </summary>
    public IEnumerable<FileReferenceModel> FileReferences { get; set; } = Array.Empty<FileReferenceModel>();

    /// <summary>
    /// get/set - The story body.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Whether the transcription has been approved.
    /// </summary>
    public bool IsApproved { get; set; }
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
        this.ContentType = entity.ContentType;
        this.FileReferences = entity.FileReferences.Select(e => new FileReferenceModel(e));
        this.Body = entity.Body;
        this.IsApproved = entity.IsApproved;
    }
    #endregion
}
