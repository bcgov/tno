using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.AVOverview;

public class AVOverviewTemplateModel : AuditColumnsModel
{
    #region Properties
    // <summary>
    // get/set - The template type.
    // </summary>
    public Entities.AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - Foreign key to report template.
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - An array of sections in this instance.
    /// </summary>
    public IEnumerable<AVOverviewTemplateSectionModel> Sections { get; set; } = Array.Empty<AVOverviewTemplateSectionModel>();

    /// <summary>
    /// get/set - An array of users subscribed to this report.
    /// </summary>
    public IEnumerable<UserModel> Subscribers { get; set; } = Array.Empty<UserModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateModel.
    /// </summary>
    public AVOverviewTemplateModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewTemplateModel(Entities.AVOverviewTemplate entity) : base(entity)
    {
        this.TemplateType = entity.TemplateType;
        this.TemplateId = entity.TemplateId;
        this.Sections = entity.Sections.OrderBy(s => s.SortOrder).Select(s => new AVOverviewTemplateSectionModel(s));
        this.Subscribers = entity.SubscribersManyToMany.Where(s => s.User != null).Select(s => new UserModel(s.User!, s.IsSubscribed, s.SendTo)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewTemplate(AVOverviewTemplateModel model)
    {
        // Always set the publishedOn to the local date in the morning.
        var entity = new Entities.AVOverviewTemplate(model.TemplateType, model.TemplateId)
        {
            Version = model.Version ?? 0
        };

        entity.Sections.AddRange(model.Sections.OrderBy(s => s.SortOrder).Select(s => (Entities.AVOverviewTemplateSection)s).ToArray());
        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(s => new Entities.UserAVOverview(s.Id, model.TemplateType, s.IsSubscribed, s.SendTo)).ToArray());

        return entity;
    }
    #endregion
}
