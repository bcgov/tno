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
    #endregion
}
