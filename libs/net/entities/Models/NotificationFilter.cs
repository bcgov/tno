namespace TNO.Entities.Models;

/// <summary>
/// NotificationFilter class, provides a model for notification filtering.
/// </summary>
public class NotificationFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include content with this status.
    /// </summary>
    public ContentStatus? Status { get; set; }

    /// <summary>
    /// get/set - Only include content with this product.
    /// </summary>
    public string? Product { get; set; }

    /// <summary>
    /// get/set - Only include content with this product.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// get/set - Only include content with this these types.
    /// </summary>
    public ContentType[] ContentTypes { get; set; } = Array.Empty<ContentType>();

    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Only include content created or updated by this user.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// get/set - An array of source IDs.
    /// </summary>
    public long[] SourceIds { get; set; } = Array.Empty<long>();

    /// <summary>
    /// get/set - Only include content with this other source.
    /// </summary>
    public string? OtherSource { get; set; }

    /// <summary>
    /// get/set - Only include content with this headline.
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// get/set - Only include content with this page name.
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// get/set - Only include content created on this date.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - Only include content created on or after this date.
    /// </summary>
    public DateTime? CreatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only included content created on or before this date.
    /// </summary>
    public DateTime? CreatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on this date.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on or after this date.
    /// </summary>
    public DateTime? UpdatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on or before this date.
    /// </summary>
    public DateTime? UpdatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on this date.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on or after this date.
    /// </summary>
    public DateTime? PublishedStartOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on or before this date.
    /// </summary>
    public DateTime? PublishedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content with the section.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set - Only include content with the edition.
    /// </summary>
    public string? Edition { get; set; }

    /// <summary>
    /// get/set - Only include content with the byline.
    /// </summary>
    public string? Byline { get; set; }

    /// <summary>
    /// get/set - Only include content with a topic.
    /// </summary>
    public bool? HasTopic { get; set; }

    /// <summary>
    /// get/set - Whether to include hidden content.
    /// </summary>
    public bool? IncludeHidden { get; set; }

    /// <summary>
    /// get/set - Whether to only return hidden content.
    /// </summary>
    public bool? OnlyHidden { get; set; }

    /// <summary>
    /// get/set - Whether to only included published or publishing content.
    /// </summary>
    public bool? OnlyPublished { get; set; }

    /// <summary>
    /// get/set - An array of content IDs.
    /// </summary>
    public long[] ContentIds { get; set; } = Array.Empty<long>();

    /// <summary>
    /// get/set - An array of content IDs.
    /// </summary>
    public long[] ProductIds { get; set; } = Array.Empty<long>();

    /// <summary>
    /// get/set - Only include content with the specified actions.
    /// </summary>
    public string[] Actions { get; set; } = Array.Empty<string>();
    #endregion
}
