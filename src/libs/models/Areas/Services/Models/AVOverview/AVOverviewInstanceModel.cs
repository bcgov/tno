using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.AVOverview;

public class AVOverviewInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    public long Id { get; set; }

    // <summary>
    // get/set - The template type.
    // </summary>
    public Entities.AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - The report template.
    /// </summary>
    public ReportTemplateModel? Template { get; set; }

    /// <summary>
    /// get/set - The published on date.
    /// </summary>
    public DateTime PublishedOn { get; set; }

    /// <summary>
    /// get/set - Whether this instance has been published (emailed).
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// get/set - An array of sections in this instance.
    /// </summary>
    public IEnumerable<AVOverviewSectionModel> Sections { get; set; } = Array.Empty<AVOverviewSectionModel>();

    // <summary>
    // get/set - The response.
    // </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - An array of users subscribed to this report.
    /// </summary>
    public IEnumerable<UserModel> Subscribers { get; set; } = Array.Empty<UserModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel.
    /// </summary>
    public AVOverviewInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="options"></param>
    public AVOverviewInstanceModel(Entities.AVOverviewInstance instance, JsonSerializerOptions options) : base(instance)
    {
        var template = instance.Template ?? throw new InvalidOperationException("AV overview instance must include the template.");
        this.Id = instance.Id;
        this.TemplateType = instance.TemplateType;
        this.Template = template.Template != null ? new ReportTemplateModel(template.Template, options) : null;
        this.PublishedOn = instance.PublishedOn;
        this.IsPublished = instance.IsPublished;
        this.Sections = instance.Sections.OrderBy(s => s.SortOrder).Select(s => new AVOverviewSectionModel(s));
        this.Response = instance.Response;
        this.Subscribers = template.SubscribersManyToMany.Where(s => s.User != null).Select(s => new UserModel(s.User!, s.IsSubscribed)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewInstance(AVOverviewInstanceModel model)
    {
        // Always set the publishedOn to the local date in the morning.
        var entity = new Entities.AVOverviewInstance(model.TemplateType, model.PublishedOn)
        {
            Id = model.Id,
            IsPublished = model.IsPublished,
            Response = model.Response,
            Version = model.Version ?? 0
        };

        entity.Sections.AddRange(model.Sections.OrderBy(s => s.SortOrder).Select(s => (Entities.AVOverviewSection)s));

        return entity;
    }
    #endregion
}
