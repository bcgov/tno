using FTTLib;

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
    public DateTime? Modified { get; set; }

    /// <summary>
    /// get/set - Whether the file/folder is locally accessible to the API.
    /// </summary>
    public bool IsLocal { get; set; }
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
    /// <param name="isLocal"></param>
    public ItemModel(string path, bool isLocal = false)
    {
        this.IsLocal = isLocal;
        this.Name = System.IO.Path.GetFileName(path);
        this.IsDirectory = (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        if (!this.IsDirectory)
        {
            var ext = System.IO.Path.GetExtension(path).Replace(".", "");
            if (!string.IsNullOrWhiteSpace(ext)) this.Extension = ext;
            var fileInfo = new System.IO.FileInfo(path);
            this.Size = fileInfo.Length;
            this.MimeType = FTT.GetMimeType(fileInfo.Name);
            this.Modified = fileInfo.LastWriteTime;
        }
        else
        {
            var info = new System.IO.DirectoryInfo(path);
            this.Modified = info.LastWriteTime;
        }
    }

    /// <summary>
    /// Creates a new instance of an ItemModel, initializes with specified parameter.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="isLocal"></param>
    public ItemModel(Renci.SshNet.Sftp.ISftpFile file, bool isLocal = false)
    {
        this.IsLocal = isLocal;
        this.Name = file.Name;
        this.IsDirectory = file.IsDirectory;
        this.Modified = file.Attributes.LastWriteTime;
        if (!this.IsDirectory)
        {
            var ext = System.IO.Path.GetExtension(file.Name).Replace(".", "");
            if (!string.IsNullOrWhiteSpace(ext)) this.Extension = ext;
            this.Size = file.Attributes.Size;
            if (!String.IsNullOrWhiteSpace(this.Extension))
                this.MimeType = FTT.GetMimeType(file.Name);
        }
    }

    /// <summary>
    /// Creates a new instance of an ItemModel, initializes with specified parameter.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="attributes"></param>
    /// <param name="isLocal"></param>
    public ItemModel(string name, Renci.SshNet.Sftp.SftpFileAttributes attributes, bool isLocal = false)
    {
        this.IsLocal = isLocal;
        this.Name = name;
        this.IsDirectory = attributes.IsDirectory;
        this.Modified = attributes.LastWriteTime;
        if (!this.IsDirectory)
        {
            var ext = System.IO.Path.GetExtension(name).Replace(".", "");
            if (!string.IsNullOrWhiteSpace(ext)) this.Extension = ext;
            this.Size = attributes.Size;
            if (!String.IsNullOrWhiteSpace(this.Extension))
                this.MimeType = FTT.GetMimeType(name);
        }
    }
    #endregion
}
