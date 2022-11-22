namespace TNO.API.Areas.Editor.Models.MorningReport;

/// <summary>
/// ContentAction enum, provides the action can be applied to content.
/// </summary>
public enum ContentListAction
{
    /// <summary>
    /// Remove the content from search.
    /// </summary>
    Remove = 0,
    /// <summary>
    /// Publish the content.
    /// </summary>
    Publish = 1,
    /// <summary>
    /// Unpublish the content.
    /// </summary>
    Unpublish = 2,
    /// <summary>
    /// Add the specified action to the content.
    /// </summary>
    Action = 3,
}
