using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public abstract class PageFilter
{
    #region Properties
    public int Page { get; set; } = 1;

    public int Quantity { get; set; } = 10;
    #endregion

    #region Constructors
    public PageFilter() { }

    public PageFilter(Dictionary<string, StringValues> queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Page = filter.GetIntValue(nameof(this.Page), 1);
        this.Quantity = filter.GetIntValue(nameof(this.Quantity), 10);
    }
    #endregion
}
