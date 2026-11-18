namespace TNO.API.Areas.Subscriber.Models.Content;

/// <summary>
/// ContentAction enum, provides the action can be applied to content.
/// </summary>
public enum ContentListAction
{
    /// <summary>
    /// Hide the content.
    /// </summary>
    Hide = 0,
    /// <summary>
    /// Unhide the content.
    /// </summary>
    Unhide = 1,
    /// <summary>
    /// Publish the content.
    /// </summary>
    Publish = 2,
    /// <summary>
    /// Unpublish the content.
    /// </summary>
    Unpublish = 3,
    /// <summary>
    /// Add the specified action to the content.
    /// </summary>
    Action = 4,
}
