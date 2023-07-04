using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.ReportTemplate;

/// <summary>
/// ChartTemplateModel class, provides a model that represents an report template.
/// </summary>
public class ChartTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor template to generate the report.
    /// </summary>
    public string Template { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartTemplateModel.
    /// </summary>
    public ChartTemplateModel() { }

    /// <summary>
    /// Creates a new instance of an ChartTemplateModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ChartTemplateModel(Entities.ChartTemplate entity) : base(entity)
    {
        this.Template = entity.Template;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ChartTemplate(ChartTemplateModel model)
    {
        var entity = new Entities.ChartTemplate(model.Id, model.Name, model.Template)
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
