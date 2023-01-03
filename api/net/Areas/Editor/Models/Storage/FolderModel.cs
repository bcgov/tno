using TNO.Core.Extensions;

namespace TNO.API.Areas.Editor.Models.Storage;

/// <summary>
/// FolderModel class, provides a model that represents an file or directory.
/// </summary>
public class FolderModel
{
    #region Properties
    /// <summary>
    /// get/set - The path to the file.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// get/set - An array of files.
    /// </summary>
    public IEnumerable<ItemModel> Items { get; set; } = Array.Empty<ItemModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FolderModel.
    /// </summary>
    public FolderModel() { }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="path"></param>
    public FolderModel(string rootPath, string path)
    {
        var safePath = System.IO.Path.Combine(rootPath, path.MakeRelativePath());
        if (!safePath.DirectoryExists()) throw new InvalidOperationException($"Folder does not exist '{safePath}'");

        this.Path = safePath.ReplaceFirst(rootPath, "")!;
        var result = new List<ItemModel>();
        var files = System.IO.Directory.GetFileSystemEntries(safePath);
        Array.Sort(files, string.CompareOrdinal);

        // Don't include temporary folder.
        foreach (var fullName in files)
        {
            result.Add(new ItemModel(fullName));
        }
        this.Items = result.ToArray();
    }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="files"></param>
    public FolderModel(IEnumerable<string> files)
    {
        var result = new List<ItemModel>();
        foreach (var fullName in files)
        {
            result.Add(new ItemModel(fullName));
        }
        this.Items = result.ToArray();
    }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="files"></param>
    public FolderModel(IEnumerable<Renci.SshNet.Sftp.SftpFile> files)
    {
        var result = new List<ItemModel>();
        foreach (var file in files)
        {
            result.Add(new ItemModel(file));
        }
        this.Items = result.ToArray();
    }
    #endregion
}
