namespace TNO.Entities;

/// <summary>
/// TopicPopulationMode enum, how Content-Analysis assigns topics to content.
/// </summary>
public enum TopicPopulationMode
{
    /// <summary>
    /// Assign existing, active staff topics only.
    /// </summary>
    ExistingOnly = 0,
    /// <summary>
    /// Assign existing topics, and create a topic when none matches.
    /// </summary>
    AllowCreate = 1,
}
