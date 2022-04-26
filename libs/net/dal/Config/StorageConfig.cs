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
    /// get/set - The current environment.
    /// </summary>
    public IWebHostEnvironment? Environment { get; set; }
    #endregion

    #region Constructors
    public StorageConfig() { }

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
    public string GetPath(IWebHostEnvironment? environment = null)
    {
        if (environment == null) environment = this.Environment;
        var path = Path.Combine(environment?.WebRootPath ?? "", this.UploadPath);
        return path.EndsWith('/') ? path : $"{path}/";
    }
    #endregion
}
