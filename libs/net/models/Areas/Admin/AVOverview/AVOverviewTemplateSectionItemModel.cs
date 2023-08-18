using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.AVOverview;

/// <summary>
/// AVOverviewTemplateSectionItemModel class, provides a model that represents an overview item.
/// </summary>
public class AVOverviewTemplateSectionItemModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The section reference.
    /// </summary>
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The item type.
    /// </summary>
    public Entities.AVOverviewItemType ItemType { get; set; }

    /// <summary>
    /// get/set - The item time.
    /// </summary>
    public string Time { get; set; } = "";

    /// <summary>
    /// get/set - The item summary.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - The item content id.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The sorting order.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateSectionItemModel.
    /// </summary>
    public AVOverviewTemplateSectionItemModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateSectionItemModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewTemplateSectionItemModel(Entities.AVOverviewTemplateSectionItem entity) : base(entity)
    {
        this.Id = entity.Id;
        this.SectionId = entity.SectionId;
        this.ItemType = entity.ItemType;
        this.Time = entity.Time;
        this.Summary = entity.Summary;
        this.SortOrder = entity.SortOrder;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewTemplateSectionItem(AVOverviewTemplateSectionItemModel model)
    {
        var entity = new Entities.AVOverviewTemplateSectionItem(model.SectionId, model.ItemType, model.Time, model.Summary)
        {
            Id = model.Id,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
