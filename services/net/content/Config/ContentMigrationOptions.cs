namespace TNO.Services.Content.Config;

public class ContentMigrationOptions
{
    #region Properties
    /// <summary>
    /// get/set - The Name of the Default Tone Pool for migrated content.
    /// </summary>
    public string DefaultTonePool { get; set; } = "Default";
    public string DefaultTagName { get; set; } = "-- UPDATE MIGRATED TAG NAME --";
    #endregion
}
