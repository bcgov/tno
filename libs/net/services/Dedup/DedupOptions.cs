namespace TNO.Services.Dedup;

/// <summary>
/// DedupOptions class, controls how stories are compared when looking for duplicates.
/// </summary>
public class DedupOptions
{
    #region Properties
    /// <summary>
    /// get/set - Whether duplicate detection is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// get/set - Only stories published within this many days of each other are compared.
    /// </summary>
    public int WindowDays { get; set; } = 7;

    /// <summary>
    /// get/set - Minimum headline word overlap (Jaccard similarity, 0 to 1) for two stories to be duplicates.
    /// </summary>
    public double HeadlineThreshold { get; set; } = 0.8;

    /// <summary>
    /// get/set - Minimum story text similarity (SimHash, 0.5 is unrelated and 1 is identical) for two stories to be duplicates.
    /// </summary>
    public double BodyThreshold { get; set; } = 0.85;

    /// <summary>
    /// get/set - Headlines with fewer significant words than this ("Letters", "Obituaries") never match on the headline alone.
    /// </summary>
    public int MinHeadlineTokens { get; set; } = 3;

    /// <summary>
    /// get/set - Story text with fewer words than this is not fingerprinted, so it can only match on the headline.
    /// </summary>
    public int MinBodyTokens { get; set; } = 40;
    #endregion
}
