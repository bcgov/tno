using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Report;

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
    /// <param name="options"></param>
    public ChartTemplateModel(Entities.ChartTemplate entity) : base(entity)
    {
        this.Template = entity.Template;
    }
    #endregion
}
