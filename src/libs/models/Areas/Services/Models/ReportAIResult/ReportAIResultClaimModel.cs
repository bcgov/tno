using System.Text.Json;

namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAIResultClaimModel class, a request to generate an AI section result. The claim succeeds
/// when no result exists for the hash, the last attempt failed, or a previous claim lapsed.
/// </summary>
public class ReportAIResultClaimModel
{
    #region Properties
    /// <summary>get/set - SHA-256 of the manifest, as hex.</summary>
    public string Hash { get; set; } = "";

    /// <summary>get/set - The report.</summary>
    public int ReportId { get; set; }

    /// <summary>get/set - The report instance being generated, if any.</summary>
    public long? ReportInstanceId { get; set; }

    /// <summary>get/set - The AI section.</summary>
    public int ReportSectionId { get; set; }

    /// <summary>get/set - The pinned manifest.</summary>
    public JsonDocument Manifest { get; set; } = JsonDocument.Parse("{}");

    /// <summary>get/set - The pipeline version.</summary>
    public string PipelineVersion { get; set; } = "";

    /// <summary>get/set - How long the claim is held before another generator may take over.</summary>
    public int LeaseSeconds { get; set; } = 900;
    #endregion
}
