using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;

namespace TNO.DAL.Config;

/// <summary>
/// StorageOptions class, provides a way to configure storage of files.
/// </summary>
public class StorageOptions
{
    #region Properties
    /// <summary>
    /// get/set - Path to folder where files will be uploaded.
    /// </summary>
    [Required]
    public string UploadPath { get; set; } = "/usr/app/data";

    /// <summary>
    /// get/set - Path to folder where audio/video capture files will be stored.
    /// </summary>
    [Required]
    public string CapturePath { get; set; } = "/usr/app/av";

    /// <summary>
    /// get/set - An array of allowed file types that can be downloaded anonymously.
    /// </summary>
    public string[] AllowAnonymousDownloadFileTypes { get; set; } = [];

    /// <summary>
    /// get/set - The current environment.
    /// </summary>
    public IWebHostEnvironment? Environment { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageOptions object.
    /// </summary>
    public StorageOptions() { }

    /// <summary>
    /// Creates a new instance of a StorageOptions object, initializes with specified parameters.
    /// </summary>
    /// <param name="environment"></param>
    public StorageOptions(IWebHostEnvironment environment)
    {
        this.Environment = environment;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get a path to the upload folder.
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    public string GetUploadPath(IWebHostEnvironment? environment = null)
    {
        environment ??= this.Environment;
        var path = Path.Combine(environment?.WebRootPath ?? "", this.UploadPath);
        return path.EndsWith('/') ? path : $"{path}/";
    }

    /// <summary>
    /// Get a path to the capture folder.
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    public string GetCapturePath(IWebHostEnvironment? environment = null)
    {
        environment ??= this.Environment;
        var path = Path.Combine(environment?.WebRootPath ?? "", this.CapturePath);
        return path.EndsWith('/') ? path : $"{path}/";
    }
    #endregion
}
