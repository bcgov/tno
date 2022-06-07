using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;

namespace TNO.DAL.Config;

/// <summary>
/// StorageConfig class, provides a way to configure storage of files.
/// </summary>
public class StorageConfig
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
    /// get/set - The current environment.
    /// </summary>
    public IWebHostEnvironment? Environment { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageConfig object.
    /// </summary>
    public StorageConfig() { }

    /// <summary>
    /// Creates a new instance of a StorageConfig object, initializes with specified parameters.
    /// </summary>
    /// <param name="environment"></param>
    public StorageConfig(IWebHostEnvironment environment)
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
        if (environment == null) environment = this.Environment;
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
        if (environment == null) environment = this.Environment;
        var path = Path.Combine(environment?.WebRootPath ?? "", this.CapturePath);
        return path.EndsWith('/') ? path : $"{path}/";
    }

    /// <summary>
    /// Get the root path based on the storage location.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public string GetRootPath(string? location)
    {
        return location?.ToLower() switch
        {
            "upload" => this.UploadPath,
            "capture" => this.CapturePath,
            _ => this.CapturePath
        };
    }
    #endregion
}
