namespace TNO.API.Config;

/// <summary>
/// ApiOptions class, provides a way to configure the API.
/// </summary>
public class ApiOptions
{
    #region Properties
    /// <summary>
    /// get/set - The name of the data location the API is running at.
    /// </summary>
    public string DataLocation { get; set; } = "";

    /// <summary>
    /// get/set - The name of the settings key to identify the notification that will be use to send transcript confirmation emails.
    /// </summary>
    public string TranscriptRequestConfirmationKey { get; set; } = "";
    #endregion
}
