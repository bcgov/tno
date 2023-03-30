namespace TNO.API.Areas.Editor.Models.Content;

/// <summary>
/// ContentListModel class, provides a model to update the morning report content items.
/// </summary>
public class ContentListModel
{
    #region Properties
    /// <summary>
    /// get/set - The action to perform on the listed content.
    /// </summary>
    public ContentListAction Action { get; set; }

    /// <summary>
    /// get/set - The name of the action to add to the content.
    /// </summary>
    public string? ActionName { get; set; }

    /// <summary>
    /// get/set - The value to apply depending on the action.
    /// </summary>
    public string? ActionValue { get; set; }

    /// <summary>
    /// get/set - An array of content IDs to perform the action on.
    /// </summary>
    public IEnumerable<long> ContentIds { get; set; } = Array.Empty<long>();
    #endregion
}
