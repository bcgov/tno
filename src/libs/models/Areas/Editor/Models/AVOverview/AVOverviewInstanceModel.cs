using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.AVOverview;

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

    // <summary>
    // get/set - The published on date.
    // </summary>
    public DateTime PublishedOn { get; set; }

    /// <summary>
    /// get/set - Whether the report has been published (emailed).
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel.
    /// </summary>
    public AVOverviewInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewInstanceModel(Entities.AVOverviewInstance entity) : base(entity)
    {
        this.Id = entity.Id;
        this.TemplateType = entity.TemplateType;
        this.PublishedOn = entity.PublishedOn;
        this.IsPublished = entity.IsPublished;
        this.Sections = entity.Sections.OrderBy(s => s.SortOrder).Select(s => new AVOverviewSectionModel(s));
        this.Response = entity.Response;
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewInstanceModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="publishedOn"></param>
    public AVOverviewInstanceModel(Entities.AVOverviewTemplate entity, DateTime publishedOn) : base(entity)
    {
        this.TemplateType = entity.TemplateType;
        this.PublishedOn = publishedOn.ToUniversalTime();
        this.Sections = entity.Sections.OrderBy(s => s.SortOrder).Select(s => new AVOverviewSectionModel(s));
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
