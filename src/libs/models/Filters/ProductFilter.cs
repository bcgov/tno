using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class ProductFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - The name of the product.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// get/set - Whether the product is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// get/set - Whether the product is publicly available.
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? SubscriberUserId { get; set; }

    /// <summary>
    /// get/set - Find all products visible to the user.
    /// </summary>
    public int? IsAvailableToUserId { get; set; }
    #endregion

    #region Constructors
    public ProductFilter() { }

    public ProductFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.IsEnabled = filter.GetBoolNullValue(nameof(this.IsEnabled));
        this.IsPublic = filter.GetBoolNullValue(nameof(this.IsPublic));
        this.SubscriberUserId = filter.GetIntNullValue(nameof(this.SubscriberUserId));
        this.IsAvailableToUserId = filter.GetIntNullValue(nameof(this.IsAvailableToUserId));
    }
    #endregion
}
