namespace TNO.API.Models.Settings;

public class FolderSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - The age limit of content that remains in the folder when cleaned.
    /// 0 days will clean out the folder entirely when schedule runs.
    /// </summary>
    public int KeepAgeLimit { get; set; }

    /// <summary>
    /// get/set - If the folder auto populates.
    /// </summary>
    public bool AutoPopulate { get; set; } = false;
    #endregion
}
