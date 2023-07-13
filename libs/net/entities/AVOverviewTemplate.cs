using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplate class, provides a DB model to manage different types of overviews.
/// </summary>
[Cache("av_overview_template")]
[Table("av_overview_template")]
public class AVOverviewTemplate : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of template.
    /// </summary>
    [Column("template_type")]
    public TemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - The template for this av overview template
    /// </summary>
    [Column("template")]
    public string Template { get; set; } = "";


    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a av overview template object.
    /// </summary>
    protected AVOverviewTemplate() : base() { }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <param name="template"></param>
    public AVOverviewTemplate(TemplateType type, string template, string name) : base(name)
    {
        this.TemplateType = type;
        this.Template = template;
    }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="ownerId"></param>
    /// <param name="ownerId"></param>
    public AVOverviewTemplate(int id, TemplateType type, string template, string name) : base(id, name)
    {
        this.TemplateType = type;
        this.Template = template;
    }
    #endregion
}
