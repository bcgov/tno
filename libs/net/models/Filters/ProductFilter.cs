using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class ProductFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    public bool? IsPublic { get; set; }
    #endregion

    #region Constructors
    public ProductFilter() { }

    public ProductFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.IsPublic = filter.GetBoolNullValue(nameof(this.IsPublic));
        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
