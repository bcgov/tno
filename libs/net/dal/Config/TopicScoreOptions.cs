namespace TNO.DAL.Config;

/// <summary>
/// TopicScoreOptions class, configuration for topic score calculation.
/// </summary>
public class TopicScoreOptions
{
    #region Properties
    /// <summary>
    /// get/set - The time zone topic score rule times are expressed in.
    /// </summary>
    public string TimeZone { get; set; } = "Pacific Standard Time";
    #endregion
}
