namespace TNO.API.Areas.Report;

/// <summary>
/// FileReferenceModel class, provides a model that represents an file reference.
/// </summary>
public class FileReferenceModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to file reference.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The file mime type.
    /// </summary>
    public string ContentType { get; set; } = "";

    /// <summary>
    /// get/set - The friendly name or the name the user gave for the file.
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// get/set - The path to the file.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// get/set - The size of the file in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// get/set - The number of seconds this file runs.
    /// </summary>
    public long RunningTime { get; set; }

    /// <summary>
    /// get/set - Whether the file has been uploaded and is available.
    /// </summary>
    public bool IsUploaded { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FileReferenceModel.
    /// </summary>
    public FileReferenceModel() { }

    /// <summary>
    /// Creates a new instance of an FileReferenceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public FileReferenceModel(Entities.FileReference entity)
    {
        this.Id = entity.Id;
        this.ContentType = entity.ContentType;
        this.FileName = entity.FileName;
        this.Path = entity.Path;
        this.Size = entity.Size;
        this.RunningTime = entity.RunningTime;
        this.IsUploaded = entity.IsUploaded;
    }

    /// <summary>
    /// Creates a new instance of an FileReferenceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FileReferenceModel(TNO.API.Areas.Services.Models.Content.FileReferenceModel model)
    {
        this.Id = model.Id;
        this.ContentType = model.ContentType;
        this.FileName = model.FileName;
        this.Path = model.Path;
        this.Size = model.Size;
        this.RunningTime = model.RunningTime;
        this.IsUploaded = model.IsUploaded;
    }
    #endregion
}
