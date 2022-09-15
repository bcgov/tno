namespace TNO.Entities;

/// <summary>
/// ContentType enum, provides a way to identify the type of content the media belongs to and which form to use.
/// </summary>
public enum ContentType
{
    /// <summary>
    /// Snippet content contains audio/video.
    /// </summary>
    Snippet = 0,
    /// <summary>
    /// Print content is stories published by newspapers in traditional media.
    /// </summary>
    PrintContent = 1,
    /// <summary>
    /// Image files and front page images of newspapers.
    /// </summary>
    Image = 2,
    /// <summary>
    /// Text based content, which can contain files.
    /// Used for internet based content with links.
    /// </summary>
    Story = 3,
}
