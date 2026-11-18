using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public abstract class PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - The page to start on.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// get/set - The number of record to return.
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// get/set - An array of property name and the direction to sort by.
    /// </summary>
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public PageFilter() { }

    public PageFilter(Dictionary<string, StringValues> queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Page = filter.GetIntNullValue(nameof(this.Page));
        this.Quantity = filter.GetIntNullValue(nameof(this.Quantity));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
