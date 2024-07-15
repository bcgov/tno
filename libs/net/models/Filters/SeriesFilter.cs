using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

public class SeriesFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsOther { get; set; }
    #endregion

    #region Constructors
    public SeriesFilter() { }

    public SeriesFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Description = filter.GetStringValue(nameof(this.Description));

        var isOtherValue = filter.GetStringValue(nameof(this.IsOther));
        if (!string.IsNullOrEmpty(isOtherValue))
            this.IsOther = bool.Parse(isOtherValue);
    }
    #endregion
}
