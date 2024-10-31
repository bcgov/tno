using TNO.Services.Config;

namespace TNO.Services.FileUpload.Config;

public class FileUploadOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The maximum number of files to upload at a time.
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// get/set - Whether to force upload.
    /// </summary>
    public bool Force { get; set; }


    /// <summary>
    /// get/set - Days before today for the start date
    /// </summary>
    public int DaysBeforeStart { get; set; } = 31;

    /// <summary>
    /// get/set - Days before today for the end date
    /// </summary>
    public int DaysBeforeEnd { get; set; } = 30;

    #endregion
}