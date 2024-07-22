using TNO.Entities;

namespace TNO.API.Models.Settings;

/// <summary>
/// FilterSettingsModel class, provides a model to control filter settings consistently.
/// </summary>
public class FilterSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Whether to search the unpublished index (which includes published content).
    /// </summary>
    public bool SearchUnpublished { get; set; }

    /// <summary>
    /// get/set - The Elasticsearch query type to use [query-string, simple-query-string]
    /// </summary>
    public string QueryType { get; set; } = "query-string";

    /// <summary>
    /// get/set - The default search query operator Elasticsearch will use [and|or]
    /// </summary>
    public string? DefaultOperator { get; set; }

    /// <summary>
    /// get/set - The number of content items to search for.
    /// Defaults to 10, limit is 10,000.
    /// </summary>
    public int Size { get; set; } = 10;

    /// <summary>
    /// get/set - The way to do paging.
    /// </summary>
    public int? From { get; set; }

    /// <summary>
    /// get/set - Filter on the content which is owned.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Filter on the content which is owned, created, or updated by this user.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// get/set - Keyword search support Elasticsearch query syntax.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// get/set - Search for the 'search' values in the headline.
    /// </summary>
    public bool? InHeadline { get; set; }

    /// <summary>
    /// get/set - Search for the 'search' values in the byline.
    /// </summary>
    public bool? InByline { get; set; }

    /// <summary>
    /// get/set - Search for the 'search' values in the abstract/body.
    /// </summary>
    public bool? InStory { get; set; }

    /// <summary>
    /// get/set - Search for the 'search' values in the abstract/body.
    /// </summary>
    public bool? InProgram { get; set; }

    /// <summary>
    /// get/set - Filter content that was published on and after this date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// get/set - Filter content that was published on and before this date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// get/set - Filter content that was posted on and after this date.
    /// </summary>
    public DateTime? StartPostedDate { get; set; }

    /// <summary>
    /// get/set - Filter content that was posted on and before this date.
    /// </summary>
    public DateTime? EndPostedDate { get; set; }

    /// <summary>
    /// get/set - Filter content that is X days older than today.
    /// </summary>
    public int? DateOffset { get; set; }

    /// <summary>
    /// get/set - Filter content with this edition.
    /// </summary>
    public string? Edition { get; set; }

    /// <summary>
    /// get/set - Filter content with this section.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set - Filter content with this page.
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// get/set - Filter content that has a topic.
    /// </summary>
    public bool? HasTopic { get; set; }

    /// <summary>
    /// get/set - Filter content that is hidden.
    /// </summary>
    public bool? IsHidden { get; set; }

    /// <summary>
    /// get/set - Filter content by other source.
    /// </summary>
    public string? OtherSource { get; set; }

    /// <summary>
    /// get/set - Filter content based on status.
    /// </summary>
    public ContentStatus? Status { get; set; }

    /// <summary>
    /// get/set - An array of source Id.
    /// </summary>
    public int[]? SourceIds { get; set; }

    /// <summary>
    /// get/set - An array of media type Id.
    /// </summary>
    public int[]? MediaTypeIds { get; set; }

    /// <summary>
    /// get/set - An array of series Id.
    /// </summary>
    public int[]? SeriesIds { get; set; }

    /// <summary>
    /// get/set - An array of contributor Id.
    /// </summary>
    public int[]? ContributorIds { get; set; }

    /// <summary>
    /// get/set - An array of actions.
    /// Includes the value of the action to filter on.
    /// </summary>
    public FilterActionSettingsModel[]? Actions { get; set; }

    /// <summary>
    /// get/set - An array of content types.
    /// </summary>
    public ContentType[]? ContentTypes { get; set; }

    /// <summary>
    /// get/set - An array of tags.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// get/set - An array of source Id.
    /// Represents a range, from and to.
    /// </summary>
    public int[]? Sentiment { get; set; }

    /// <summary>
    /// get/set - An array of content Id.
    /// </summary>
    public long[]? ContentIds { get; set; }

    /// <summary>
    /// get/set - An array of sort conditions (i.e. { name: 'desc' })
    /// </summary>
    public dynamic[]? Sort { get; set; }
    #endregion
}
