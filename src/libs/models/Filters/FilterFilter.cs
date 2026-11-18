using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
namespace TNO.Models.Filters;

public class FilterFilter : PageFilter
{
    #region Properties
    public int? OwnerId { get; set; }
    public string? Name { get; set; }

    /// <summary>
    /// get/set - Text found in the name, description, or the owner's username, display name, first or last name.
    /// </summary>
    public string? Keyword { get; set; }
    #endregion

    #region Constructors
    public FilterFilter() { }

    public FilterFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Keyword = filter.GetStringValue(nameof(this.Keyword));
    }
    #endregion
}
