using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class ProductFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public ProductFilter() { }

    public ProductFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
