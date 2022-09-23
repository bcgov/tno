using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Models;

public class CategoryFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    /// <summary>
    /// get/set - The type of category (issue, proactive).
    /// </summary>
    public CategoryType? CategoryType { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public CategoryFilter() { }

    public CategoryFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Description = filter.GetStringValue(nameof(this.Description));

        this.CategoryType = filter.GetEnumNullValue<CategoryType>(nameof(this.CategoryType));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
