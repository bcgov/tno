using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// AVOverviewInstance class, provides a DB model to manage different types of overviews.
/// </summary>
[Table("av_overview_instance")]
public class AVOverviewInstance : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of template.
    /// </summary>
    [Column("template_type")]
    public TemplateType TemplateType { get; set; }

    [Column("published_on")]
    /// <summary>
    /// get/set - When the content has been or will be published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The response.
    /// </summary>
    [Column("response")]
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");


    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a av overview template object.
    /// </summary>
    protected AVOverviewInstance() : base() { }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <param name="template"></param>
    public AVOverviewInstance(string name) : base(name)
    {

    }

    #endregion
}
