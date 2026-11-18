namespace TNO.Services.Dedup;

/// <summary>
/// DedupMatch class, the story a candidate duplicates and why.
/// </summary>
/// <param name="Primary">The story that was kept.</param>
/// <param name="Reason">Why the candidate is a duplicate.</param>
/// <param name="Score">Similarity score of the comparison that matched (1 is identical).</param>
public record DedupMatch(DedupEntry Primary, DedupMatchReason Reason, double Score);
