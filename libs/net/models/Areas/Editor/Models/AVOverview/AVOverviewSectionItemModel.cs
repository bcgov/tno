using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.AvOverview;

/// <summary>
/// AVOverviewSectionitem class, provides a model that represents an overview item.
/// </summary>
public class AVOverviewSectionItemModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    
    /// <summary>
    /// get/set - The section reference.
    /// </summary>
    public int AVOverviewSectionId { get; set; }

    /// <summary>
    /// get/set - The item type.    
    /// </summary>
    public AVOverviewItemType ItemType { get; set; }

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
    /// <param name="options"></param>
    public AVOverviewSectionItemModel(Entities.AVOverviewSectionItem entity, JsonSerializerOptions options) : base(entity)
    {
        this.AVOverviewSectionId = entity.AVOverviewSectionId;
        this.ItemType = entity.ItemType;
        this.Time = entity.Time;
        this.Summary = entity.Summary;
        this.ContentId = entity.ContentId;
        
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a AVOverviewSectionModel object.
    /// </summary>
    /// <returns></returns>
    public Entities.AVOverviewSectionItem ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.AVOverviewSectionItem)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewSectionItem(AVOverviewSectionItemModel model)
    {
        var entity = new Entities.AVOverviewSectionItem(model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };


        return entity;
    }
    #endregion
}