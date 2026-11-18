namespace TNO.API.Areas.Services.Models.TopicScore;

/// <summary>
/// TopicRescoreConfigurationModel class, a bulk rescore's criteria and progress, kept in its work
/// order's configuration. The start is inclusive and the end exclusive, both UTC.
/// </summary>
public class TopicRescoreConfigurationModel
{
    /// <summary>get/set - Content published on or after this date.</summary>
    public DateTime StartOn { get; set; }

    /// <summary>get/set - Content published before this date.</summary>
    public DateTime EndOn { get; set; }

    /// <summary>get/set - The sources included; empty is every source.</summary>
    public int[] SourceIds { get; set; } = Array.Empty<int>();

    /// <summary>get/set - The last content ID rescored; the next page starts after it.</summary>
    public long CheckpointContentId { get; set; }

    /// <summary>get/set - Content in the range when the rescore started.</summary>
    public int Total { get; set; }

    /// <summary>get/set - Content processed so far.</summary>
    public int Processed { get; set; }

    /// <summary>get/set - Content whose score changed.</summary>
    public int Changed { get; set; }

    /// <summary>get/set - Content that could not be rescored.</summary>
    public int Failed { get; set; }

    /// <summary>get/set - The last error.</summary>
    public string? Error { get; set; }
}

/// <summary>
/// TopicRescorePageModel class, the outcome of rescoring one page of a bulk rescore.
/// </summary>
public class TopicRescorePageModel
{
    /// <summary>get/set - Content processed in the page.</summary>
    public int Processed { get; set; }

    /// <summary>get/set - Content whose score changed (and is re-indexed).</summary>
    public int Changed { get; set; }

    /// <summary>get/set - Content that could not be rescored.</summary>
    public int Failed { get; set; }

    /// <summary>get/set - The last error in the page.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - The last content ID in the page; the next page starts after it.</summary>
    public long LastContentId { get; set; }

    /// <summary>get/set - Whether no content is left after this page.</summary>
    public bool IsLast { get; set; }
}
