using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class ProductFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public bool? IsPublic { get; set; }

    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? SubscriberUserId { get; set; }
    #endregion

    #region Constructors
    public ProductFilter() { }

    public ProductFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.SubscriberUserId = filter.GetIntNullValue(nameof(this.SubscriberUserId));
        this.IsPublic = filter.GetBoolNullValue(nameof(this.IsPublic));
    }
    #endregion
}
