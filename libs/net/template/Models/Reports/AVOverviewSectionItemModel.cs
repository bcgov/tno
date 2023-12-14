using TNO.API.Models;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// AVOverviewSectionItemModel class, provides a model that represents an overview item.
/// </summary>
public class AVOverviewSectionItemModel
{
    #region Properties
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
    /// get/set - The item content.
    /// </summary>
    public ContentModel? Content { get; set; }

    /// <summary>
    /// get/set - The sorting order.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewSectionItemModel.
    /// </summary>
    public AVOverviewSectionItemModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionItemModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewSectionItemModel(Entities.AVOverviewSectionItem entity)
    {
        this.SortOrder = entity.SortOrder;
        this.SectionId = entity.SectionId;
        this.ItemType = entity.ItemType;
        this.Time = entity.Time;
        this.Summary = entity.Summary;
        this.ContentId = entity.ContentId;

        if (entity.Content != null)
            this.Content = new ContentModel(entity.Content);
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionItemModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewSectionItemModel(TNO.API.Areas.Editor.Models.AVOverview.AVOverviewSectionItemModel model)
    {
        this.SortOrder = model.SortOrder;
        this.SectionId = model.SectionId;
        this.ItemType = model.ItemType;
        this.Time = model.Time;
        this.Summary = model.Summary;
        this.ContentId = model.ContentId;
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionItemModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewSectionItemModel(TNO.API.Areas.Services.Models.AVOverview.AVOverviewSectionItemModel model)
    {
        this.SortOrder = model.SortOrder;
        this.SectionId = model.SectionId;
        this.ItemType = model.ItemType;
        this.Time = model.Time;
        this.Summary = model.Summary;
        this.ContentId = model.ContentId;
    }
    #endregion
}
