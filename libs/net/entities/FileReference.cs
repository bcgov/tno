using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// FileReference class, provides a way to save a file reference into the database.
/// </summary>
[Table("file_reference")]
public class FileReference : AuditColumns, IReadonlyFileReference, IEquatable<FileReference>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify the file reference (Identity Seed).
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the owning content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Content that owns this file reference.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - The mime-type or content type of the file.
    /// </summary>
    [Column("content_type")]
    public string ContentType { get; set; } = "";

    /// <summary>
    /// get/set - The friendly or user defined file name.
    /// </summary>
    [Column("file_name")]
    public string FileName { get; set; } = "";

    /// <summary>
    /// get/set - The path the file has been stored at.
    /// </summary>
    [Column("path")]
    public string Path { get; set; } = "";

    /// <summary>
    /// get/set - The size of the file in bytes.
    /// </summary>
    [Column("size")]
    public long Size { get; set; }

    /// <summary>
    /// get/set - The length of running time of the file in milliseconds.
    /// </summary>
    [Column("running_time")]
    public long RunningTime { get; set; }

    /// <summary>
    /// get/set - Whether the file has been uploaded or not.
    /// </summary>
    [Column("is_uploaded")]
    public bool IsUploaded { get; set; }


    /// <summary>
    /// get/set - Whether the file has been synced to S3 or not.
    /// </summary>
    [Column("is_synced_to_s3")]
    public bool IsSyncedToS3 { get; set; }

    /// <summary>
    /// get/set - The path the file has been stored at in S3.
    /// </summary>
    [Column("s3_path")]
    public string? S3Path { get; set; } = "";

    /// <summary>
    /// get/set - The date and time the file was last synced to S3.
    /// </summary>
    [Column("last_synced_to_s3_on")]
    public DateTime? LastSyncedToS3On { get; set; }


    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileReference object.
    /// </summary>
    protected FileReference() { }

    /// <summary>
    /// Creates a new instance of a FileReference object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="contentType"></param>
    /// <param name="fileName"></param>
    /// <exception cref="ArgumentException"></exception>
    public FileReference(long contentId, string contentType, string fileName) : this(contentId, contentType, fileName, String.Empty)
    {
    }

    /// <summary>
    /// Creates a new instance of a FileReference object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="fileName"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public FileReference(Content content, string contentType, string fileName) : this(content, contentType, fileName, String.Empty)
    {
    }

    /// <summary>
    /// Creates a new instance of a FileReference object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="contentType"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <exception cref="ArgumentException"></exception>
    public FileReference(long contentId, string contentType, string fileName, string path)
    {
        if (String.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(contentType));
        if (String.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(fileName));

        this.ContentId = contentId;
        this.ContentType = contentType;
        this.FileName = fileName;
        this.Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    // The constructor order here might make a difference for Elasticsearch!
    /// <summary>
    /// Creates a new instance of a FileReference object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public FileReference(Content content, string contentType, string fileName, string path)
    {
        if (String.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(contentType));
        if (String.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(fileName));

        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.ContentType = contentType;
        this.FileName = fileName;
        this.Path = path ?? throw new ArgumentNullException(nameof(path));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determine if the file references are equal.
    /// WARNING - Only checks if the primary keys are the same.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(FileReference? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    /// <summary>
    /// Determine if the file references are equal.
    /// WARNING - Only checks if the primary keys are the same.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) => Equals(obj as FileReference);

    /// <summary>
    /// Generate a unique hash code to identify this file reference.
    /// WARNING - Only uses the primary key.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => (this.Id).GetHashCode();
    #endregion
}
