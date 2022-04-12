namespace TNO.Entities;

/// <summary>
/// Provides content status to determine the stage the content is in.
/// Content status represents either what process should be performed.
/// </summary>
public enum ContentStatus
{
    /// <summary>
    /// Content will not be published.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Content added to queue to publish.
    /// </summary>
    Publish = 1,

    /// <summary>
    /// Content has been published.
    /// </summary>
    Published = 2,

    /// <summary>
    /// Content has been requested to be unpublished.
    /// </summary>
    Unpublish = 3,

    /// <summary>
    /// Content has been unpublished.
    /// </summary>
    Unpublished = 4
}