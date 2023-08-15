using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

/// <summary>
/// ReportFilter class, provides a model for searching content.
/// </summary>
public class ReportFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The name of the report.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// get/set - Whether the report is public.
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// get/set - Whether the report is public or the user is the owner.
    /// </summary>
    public bool? IsPublicOrOwner { get; set; }
    #endregion

    #region Constructors
    public ReportFilter() { }

    public ReportFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.IsPublic = filter.GetBoolNullValue(nameof(this.IsPublic));
        this.IsPublicOrOwner = filter.GetBoolNullValue(nameof(this.IsPublicOrOwner));
    }
    #endregion
}
