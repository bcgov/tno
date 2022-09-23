using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

public class IngestTypeFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public IngestTypeFilter() { }

    public IngestTypeFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Description = filter.GetStringValue(nameof(this.Description));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
