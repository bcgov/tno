using Microsoft.AspNetCore.Http;
using MimeTypes;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ContentFileReference class, provides a way to associate and upload a file with the file reference.
/// A file cannot be uploaded until the 'Content' exists in the database, because the 'Content.Id' is required to uniquely name the file.
/// </summary>
public class ContentFileReference : IReadonlyFileReference
{
    #region Properties
    /// <summary>
    /// get - Primary key to identify this file reference (Identity Seed).
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// get - Foreign key to the owning content.
    /// </summary>
    public long ContentId { get; }

    /// <summary>
    /// get - The content that owns this file reference.
    /// </summary>
    public virtual Content Content { get; }

    /// <summary>
    /// get - The mime-type or content-type of the file.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// get - The friendly or user specified file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// get - The full path to the location the file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// get - The size of the file in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// get - The length of running time in milliseconds
    /// </summary>
    public long RunningTime { get; }

    /// <summary>
    /// get - Whether the file has been uploaded or not.
    /// </summary>
    public bool IsUploaded { get; set; }

    /// <summary>
    /// get - The file to upload with the file reference.
    /// </summary>
    public IFormFile? File { get; }

    /// <summary>
    /// get - The content version.
    /// </summary>
    public long ContentVersion { get; }

    /// <summary>
    /// get -
    /// </summary>
    public Guid CreatedById { get; }

    /// <summary>
    /// get -
    /// </summary>
    public string CreatedBy { get; }

    /// <summary>
    /// get -
    /// </summary>
    public DateTime CreatedOn { get; }

    /// <summary>
    /// get -
    /// </summary>
    public Guid UpdatedById { get; }

    /// <summary>
    /// get -
    /// </summary>
    public string UpdatedBy { get; }

    /// <summary>
    /// get -
    /// </summary>
    public DateTime UpdatedOn { get; }

    /// <summary>
    /// get - The file reference version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// get - A reference to the source FileReference.
    /// </summary>
    protected FileReference FileReference { get; }

    /// <summary>
    /// get - The file to attach with the file reference.
    /// </summary>
    public string SourceFile { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentFileReference, initializes with specified parameters.
    /// Use this to create a new FileReference.
    /// Only use this constructor when the 'content' already exists in the database.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentFileReference(Content content, IFormFile file)
    {
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
        this.ContentVersion = content.Version;

        var ext = System.IO.Path.GetExtension(file.FileName).Replace(".", "");
        this.File = file ?? throw new ArgumentNullException(nameof(file));
        this.Path = GenerateFilePath(content, file);
        this.FileName = file.FileName;
        this.ContentType = !String.IsNullOrWhiteSpace(file.ContentType) ? file.ContentType : MimeTypeMap.GetMimeType(ext);
        this.Size = file.Length;
        this.RunningTime = 0; // TODO: Calculate this somehow.
        this.CreatedBy = content.CreatedBy;
        this.CreatedById = content.CreatedById;
        this.CreatedOn = DateTime.UtcNow;
        this.UpdatedBy = content.UpdatedBy;
        this.UpdatedById = content.UpdatedById;
        this.UpdatedOn = DateTime.UtcNow;
        this.SourceFile = "";

        this.FileReference = new FileReference(this.Content, this.ContentType, this.FileName, this.Path)
        {
            IsUploaded = this.IsUploaded,
            Size = this.Size,
            RunningTime = this.RunningTime
        };
    }

    /// <summary>
    /// Creates a new instance of a ContentFileReference, initializes with specified parameters.
    /// Use this to update an existing FileReference.
    /// Only use this constructor when the 'content' already exists in the database.
    /// </summary>
    /// <param name="fileReference"></param>
    /// <param name="file"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentFileReference(FileReference fileReference, IFormFile file)
    {
        this.Content = fileReference?.Content ?? throw new ArgumentNullException(nameof(fileReference), "Parameter 'fileReference' and 'fileReference.Content' cannot be null");
        this.ContentId = fileReference.ContentId;
        this.ContentVersion = fileReference.Content.Version;

        this.File = file ?? throw new ArgumentNullException(nameof(file));
        this.Id = fileReference.Id;
        this.FileName = file.FileName;
        this.Path = GenerateFilePath(this.Content, file);
        this.ContentType = file.ContentType;
        this.Size = file.Length;
        this.RunningTime = fileReference.RunningTime; // TODO: Calculate this somehow.
        this.CreatedBy = fileReference.CreatedBy;
        this.CreatedById = fileReference.CreatedById;
        this.CreatedOn = fileReference.CreatedOn;
        this.UpdatedBy = fileReference.UpdatedBy;
        this.UpdatedById = fileReference.UpdatedById;
        this.UpdatedOn = fileReference.UpdatedOn;
        this.Version = fileReference.Version;
        this.SourceFile = "";

        fileReference.FileName = this.FileName;
        fileReference.Path = this.Path;
        fileReference.Size = this.Size;
        fileReference.ContentType = this.ContentType;
        this.FileReference = fileReference;
    }

    /// <summary>
    /// Creates a new instance of a ContentFileReference, initializes with specified parameters.
    /// Use this to create a new FileReference.
    /// Only use this contructor when the 'content' already exists in the database.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentFileReference(Content content, System.IO.FileInfo file)
    {
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
        this.ContentVersion = content.Version;

        this.Path = GenerateFilePath(content, file.FullName);
        this.SourceFile = file.FullName;
        this.ContentType = MimeTypeMap.GetMimeType(file.Extension);
        this.FileName = file.Name;
        this.Size = file.Length;
        this.RunningTime = 0; // TODO: Calculate this somehow.
        this.CreatedBy = content.CreatedBy;
        this.CreatedById = content.CreatedById;
        this.CreatedOn = DateTime.UtcNow;
        this.UpdatedBy = content.UpdatedBy;
        this.UpdatedById = content.UpdatedById;
        this.UpdatedOn = DateTime.UtcNow;

        this.FileReference = new FileReference(this.Content, this.ContentType, this.FileName, this.Path)
        {
            IsUploaded = this.IsUploaded,
            Size = this.Size,
            RunningTime = this.RunningTime
        };
    }

    /// <summary>
    /// Creates a new instance of a ContentFileReference, initializes with specified parameters.
    /// Use this to update an existing FileReference.
    /// Only use this contructor when the 'content' already exists in the database.
    /// </summary>
    /// <param name="fileReference"></param>
    /// <param name="file"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentFileReference(FileReference fileReference, System.IO.FileInfo file)
    {
        this.Content = fileReference?.Content ?? throw new ArgumentNullException(nameof(fileReference), "Parameter 'fileReference' and 'fileReference.Content' cannot be null");
        this.ContentId = fileReference.ContentId;
        this.ContentVersion = fileReference.Content.Version;

        this.Id = fileReference.Id;
        this.FileName = file.Name;
        this.Path = this.Path = GenerateFilePath(this.Content, file.FullName);
        this.SourceFile = file.FullName;
        this.ContentType = MimeTypeMap.GetMimeType(file.Extension);
        this.Size = file.Length;
        this.RunningTime = fileReference.RunningTime; // TODO: Calculate this somehow.
        this.CreatedBy = fileReference.CreatedBy;
        this.CreatedById = fileReference.CreatedById;
        this.CreatedOn = fileReference.CreatedOn;
        this.UpdatedBy = fileReference.UpdatedBy;
        this.UpdatedById = fileReference.UpdatedById;
        this.UpdatedOn = fileReference.UpdatedOn;
        this.Version = fileReference.Version;

        fileReference.FileName = this.FileName;
        fileReference.Path = this.Path;
        fileReference.Size = this.Size;
        fileReference.ContentType = this.ContentType;
        this.FileReference = fileReference;
    }

    #endregion

    #region Methods
    /// <summary>
    /// Generate the file path and name.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GenerateFilePath(Content content, IFormFile file)
    {
        var fileName = GenerateFileName(content, file);
        return System.IO.Path.Combine(content.Source, fileName);
    }

    /// <summary>
    /// Generate a unique name based on the content information.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GenerateFileName(Content content, IFormFile file)
    {
        if (content.Id == 0) throw new ArgumentException("Parameter 'content.Id' must be greater than zero.", nameof(content));
        if (String.IsNullOrWhiteSpace(content.Source)) throw new ArgumentException("Parameter 'content.Source' cannot be null, empty, or whitespace.", nameof(content));

        return $"{content.Source}-{content.Id}{System.IO.Path.GetExtension(file.FileName)}";
    }

    public static string GenerateFilePath(Content content, string file)
    {
        var fileName = GenerateFileName(content, file);
        return System.IO.Path.Combine(content.Source, fileName);
    }

    public static string GenerateFileName(Content content, string file)
    {
        if (content.Id == 0) throw new ArgumentException("Parameter 'content.Id' must be greater than zero.", nameof(content));
        if (String.IsNullOrWhiteSpace(content.Source)) throw new ArgumentException("Parameter 'content.Source' cannot be null, empty, or whitespace.", nameof(content));

        return $"{content.Source}-{content.Id}{System.IO.Path.GetExtension(file)}";
    }



    /// <summary>
    /// Cast this ContentFileReference to a new instance of a FileReference object.
    /// </summary>
    /// <param name="obj"></param>
    public static explicit operator FileReference(ContentFileReference obj)
    {
        var fileReference = obj.FileReference;
        fileReference.FileName = obj.FileName;
        fileReference.Path = obj.Path;
        fileReference.Size = obj.Size;
        fileReference.RunningTime = obj.RunningTime;
        fileReference.ContentType = obj.ContentType;
        fileReference.IsUploaded = obj.IsUploaded;
        fileReference.Version = obj.Version;
        return fileReference;
    }
    #endregion
}
