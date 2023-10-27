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
    /// get/set - The number of content items to search for.
    /// Defaults to 10, limit is 10,000.
    /// </summary>
    public int Size { get; set; } = 10;

    /// <summary>
    /// get/set - Keyword search support Elasticsearch query syntax.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// get/set - The default search query operator Elasticsearch will use [and|or]
    /// </summary>
    public string? DefaultSearchOperator { get; set; }

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
    /// get/set - Filter content that was published on and after this date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// get/set - Filter content that was published on and before this date.
    /// </summary>
    public DateTime? EndDate { get; set; }

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
    /// get/set - An array of source Id.
    /// </summary>
    public int[] SourceIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of product Id.
    /// </summary>
    public int[] ProductIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of series Id.
    /// </summary>
    public int[] SeriesIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of contributor Id.
    /// </summary>
    public int[] ContributorIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of actions.
    /// Includes the value of the action to filter on.
    /// </summary>
    public FilterActionSettingsModel[] Actions { get; set; } = Array.Empty<FilterActionSettingsModel>();

    /// <summary>
    /// get/set - An array of content types.
    /// </summary>
    public ContentType[] ContentTypes { get; set; } = Array.Empty<ContentType>();

    /// <summary>
    /// get/set - An array of tags.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - An array of source Id.
    /// Represents a range, from and to.
    /// </summary>
    public int[] Sentiment { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of sort conditions (i.e. { name: 'desc' })
    /// </summary>
    public dynamic[] Sort { get; set; } = Array.Empty<dynamic>();
    #endregion
}
