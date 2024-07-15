using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class ActionFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    #endregion

    #region Constructors
    public ActionFilter() { }

    public ActionFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Description = filter.GetStringValue(nameof(this.Description));
    }
    #endregion
}
