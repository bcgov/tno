using MimeTypes;
using System.Globalization;

namespace TNO.API.Areas.Editor.Models.Storage;

/// <summary>
/// ItemModel class, provides a model that represents an file or directory.
/// </summary>
public class ItemModel
{
    #region Properties
    /// <summary>
    /// get/set - The file name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The file extension.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// get/set - Whether this file is a directory.
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// get/set - The size in bytes of the file.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// get/set - The media-type of the file.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// get/set - The media-type of the file.
    /// </summary>
    public string? Modified { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ItemModel.
    /// </summary>
    public ItemModel() { }

    /// <summary>
    /// Creates a new instance of an ItemModel, initializes with specified parameter.
    /// </summary>
    /// <param name="path"></param>
    public ItemModel(string path)
    {
        this.Name = System.IO.Path.GetFileName(path);
        var ext = System.IO.Path.GetExtension(path).Replace(".", "");
        if (!string.IsNullOrWhiteSpace(ext)) this.Extension = ext;
        this.IsDirectory = (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        if (!this.IsDirectory)
        {
            var info = new System.IO.FileInfo(path);
            this.Size = info.Length;
            this.MimeType = MimeTypeMap.GetMimeType(this.Extension);
            this.Modified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
        }
    }
    #endregion
}
