using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.AvOverview;

public class AVOverviewInstanceModel : BaseTypeWithAuditColumnsModel<int>  
{
    #region Properties
    // <summary>
    // get/set - The template type.
    // </summary>
    public TemplateType TemplateType { get; set; }

    // <summary>
    // get/set - The published on date.
    // </summary>
    public DateTime? PublishedOn { get; set; }

    // <summary>
    // get/set - The response.
    // </summary>
    public Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();

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
    /// <param name="options"></param>
    public AVOverviewInstanceModel(Entities.AVOverviewInstance entity, JsonSerializerOptions options) : base(entity)
    {
        this.TemplateType = entity.TemplateType;
        this.PublishedOn = entity.PublishedOn;
        this.Response = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Response, options) ?? new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a AVOverviewInstanceModel object.
    /// </summary>
    /// <returns></returns>
    public Entities.AVOverviewInstance ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.AVOverviewInstance)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewInstance(AVOverviewInstanceModel model)
    {
        var entity = new Entities.AVOverviewInstance(model.Name)
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