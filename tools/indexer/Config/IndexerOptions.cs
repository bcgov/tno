using TNO.Services.Config;

namespace TNO.Tools.ElasticIndexer.Config;

public class IndexerOptions : ServiceOptions
{
    /// <summary>
    /// get/set - The page to start indexing
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// get/set - The page to stop indexing
    /// </summary>
    public int? MaxPage { get; set; }

    /// <summary>
    /// get/set - Number of content items to index at one time.
    /// </summary>
    public int Quantity { get; set; } = 100;

    /// <summary>
    /// get/set - Only include content created on or after this date.
    /// </summary>
    public DateTime? CreatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only included content created on or before this date.
    /// </summary>
    public DateTime? CreatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on or after this date.
    /// </summary>
    public DateTime? PublishedStartOn { get; set; }

    /// <summary>
    /// get/set - Only included content published on or before this date.
    /// </summary>
    public DateTime? PublishedEndOn { get; set; }

    /// <summary>
    /// get/set - An array of source IDs.
    /// </summary>
    public long[] SourceIds { get; set; } = [];
}
