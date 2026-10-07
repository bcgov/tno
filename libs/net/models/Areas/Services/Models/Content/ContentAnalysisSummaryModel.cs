using System.Text.Json;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentAnalysisSummaryModel class, the analysis carried in a content item's search document and
/// given to report synthesis. Its shape matches the strict 'analysis' mapping in Elasticsearch, so
/// properties must not be added here without a mapping migration.
/// </summary>
public class ContentAnalysisSummaryModel
{
    #region Properties
    /// <summary>get/set - The analysis.</summary>
    public long Id { get; set; }

    /// <summary>get/set - The summary.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - The primary topic label.</summary>
    public string? PrimaryTopic { get; set; }

    /// <summary>get/set - The label reports group by: the matched staff topic, otherwise the registry label.</summary>
    public string? TopicLabel { get; set; }

    /// <summary>get/set - The matched staff topic.</summary>
    public int? StaffTopicId { get; set; }

    /// <summary>get/set - When it was analyzed.</summary>
    public DateTime AnalyzedOn { get; set; }

    /// <summary>get/set - The model deployment.</summary>
    public string Model { get; set; } = "";

    /// <summary>get/set - The output schema version.</summary>
    public string SchemaVersion { get; set; } = "";

    /// <summary>get/set - Key facts.</summary>
    public IEnumerable<AnalysisFactSummaryModel> KeyFacts { get; set; } = Array.Empty<AnalysisFactSummaryModel>();

    /// <summary>get/set - People and organizations.</summary>
    public IEnumerable<AnalysisEntitySummaryModel> Entities { get; set; } = Array.Empty<AnalysisEntitySummaryModel>();

    /// <summary>get/set - Places.</summary>
    public IEnumerable<AnalysisPlaceSummaryModel> Places { get; set; } = Array.Empty<AnalysisPlaceSummaryModel>();

    /// <summary>get/set - Topics.</summary>
    public IEnumerable<AnalysisTopicSummaryModel> Topics { get; set; } = Array.Empty<AnalysisTopicSummaryModel>();

    /// <summary>get/set - Reported events.</summary>
    public IEnumerable<AnalysisEventSummaryModel> Events { get; set; } = Array.Empty<AnalysisEventSummaryModel>();

    /// <summary>get/set - Verbatim quotes.</summary>
    public IEnumerable<AnalysisQuoteSummaryModel> Quotes { get; set; } = Array.Empty<AnalysisQuoteSummaryModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisSummaryModel.
    /// </summary>
    public ContentAnalysisSummaryModel() { }

    /// <summary>
    /// Creates a new instance of a ContentAnalysisSummaryModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ContentAnalysisSummaryModel(Entities.ContentAnalysis entity, JsonSerializerOptions options)
    {
        this.Id = entity.Id;
        this.Summary = entity.Summary;
        this.PrimaryTopic = entity.PrimaryTopic;
        this.StaffTopicId = entity.AnalysisTopic?.TopicId;
        this.TopicLabel = entity.AnalysisTopic?.Topic?.Name ?? entity.AnalysisTopic?.Label ?? entity.PrimaryTopic;
        this.AnalyzedOn = entity.AnalyzedOn;
        this.Model = entity.Model;
        this.SchemaVersion = entity.SchemaVersion;
        this.KeyFacts = entity.KeyFacts.Deserialize<AnalysisFactSummaryModel[]>(options) ?? Array.Empty<AnalysisFactSummaryModel>();
        this.Entities = entity.Entities.Deserialize<AnalysisEntitySummaryModel[]>(options) ?? Array.Empty<AnalysisEntitySummaryModel>();
        this.Places = entity.Places.Deserialize<AnalysisPlaceSummaryModel[]>(options) ?? Array.Empty<AnalysisPlaceSummaryModel>();
        this.Topics = entity.Topics.Deserialize<AnalysisTopicSummaryModel[]>(options) ?? Array.Empty<AnalysisTopicSummaryModel>();
        this.Events = entity.Events.Deserialize<AnalysisEventSummaryModel[]>(options) ?? Array.Empty<AnalysisEventSummaryModel>();
        this.Quotes = entity.Quotes.Deserialize<AnalysisQuoteSummaryModel[]>(options) ?? Array.Empty<AnalysisQuoteSummaryModel>();
    }
    #endregion
}

/// <summary>A key fact.</summary>
public class AnalysisFactSummaryModel
{
    /// <summary>get/set - The fact.</summary>
    public string Statement { get; set; } = "";
    /// <summary>get/set - Whether it is inferred.</summary>
    public bool IsInferred { get; set; }
}

/// <summary>A person or organization.</summary>
public class AnalysisEntitySummaryModel
{
    /// <summary>get/set - 'person' or 'organization'.</summary>
    public string Type { get; set; } = "";
    /// <summary>get/set - The name.</summary>
    public string Name { get; set; } = "";
    /// <summary>get/set - Other names.</summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();
    /// <summary>get/set - Roles.</summary>
    public string[] Roles { get; set; } = Array.Empty<string>();
}

/// <summary>A place.</summary>
public class AnalysisPlaceSummaryModel
{
    /// <summary>get/set - The place.</summary>
    public string Name { get; set; } = "";
    /// <summary>get/set - Its role.</summary>
    public string Role { get; set; } = "";
}

/// <summary>A topic.</summary>
public class AnalysisTopicSummaryModel
{
    /// <summary>get/set - The label.</summary>
    public string Label { get; set; } = "";
    /// <summary>get/set - How central it is.</summary>
    public double Relevance { get; set; }
}

/// <summary>A reported event.</summary>
public class AnalysisEventSummaryModel
{
    /// <summary>get/set - Who acted.</summary>
    public string Actor { get; set; } = "";
    /// <summary>get/set - What they did.</summary>
    public string Action { get; set; } = "";
    /// <summary>get/set - When.</summary>
    public string? Date { get; set; }
    /// <summary>get/set - Where.</summary>
    public string? Location { get; set; }
}

/// <summary>A verbatim quote.</summary>
public class AnalysisQuoteSummaryModel
{
    /// <summary>get/set - The exact words.</summary>
    public string Statement { get; set; } = "";
    /// <summary>get/set - Who said it.</summary>
    public string Speaker { get; set; } = "";
}
