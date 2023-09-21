using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// ReportInstanceModel class, provides a model that represents an report instance.
/// </summary>
public class ReportInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report definition.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the owner of the instance.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The report.
    /// </summary>
    public Report.ReportModel? Report { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The date and time the report was sent on.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The compiled subject of the report.
    /// Used to recreate the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The compiled body of the report.
    /// Used to recreate the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - Collection of content associated with this report instance.
    /// </summary>
    public IEnumerable<ReportInstanceContentModel> Content { get; set; } = Array.Empty<ReportInstanceContentModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportInstanceModel.
    /// </summary>
    public ReportInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an ReportInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportInstanceModel(Entities.ReportInstance entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.ReportId = entity.ReportId;
        this.OwnerId = entity.OwnerId;
        this.Report = entity.Report != null ? new Report.ReportModel(entity.Report, options) : null;
        this.PublishedOn = entity.PublishedOn;
        this.SentOn = entity.SentOn;
        this.Response = entity.Response;
        this.Subject = entity.Subject;
        this.Body = entity.Body;

        this.Content = entity.ContentManyToMany.OrderBy(c => c.SectionName).ThenBy(c => c.SortOrder).Select(m => new ReportInstanceContentModel(m)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ReportInstance(ReportInstanceModel model)
    {
        var entity = new Entities.ReportInstance(model.ReportId, model.OwnerId, model.Content.Select(c => (Entities.ReportInstanceContent)c).ToArray())
        {
            Id = model.Id,
            PublishedOn = model.PublishedOn,
            SentOn = model.SentOn,
            Response = model.Response,
            Subject = model.Subject,
            Body = model.Body,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
