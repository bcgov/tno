using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

public class SourceFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public int? LicenseId { get; set; }
    public int? OwnerId { get; set; }
    public int? ProductId { get; set; }
    public string[] Actions { get; set; } = Array.Empty<string>();
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public SourceFilter() { }

    public SourceFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Code = filter.GetStringValue(nameof(this.Code));
        this.ShortName = filter.GetStringValue(nameof(this.ShortName));

        this.LicenseId = filter.GetIntNullValue(nameof(this.LicenseId));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.ProductId = filter.GetIntNullValue(nameof(this.ProductId));

        this.Actions = filter.GetStringArrayValue(nameof(this.Actions));
        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
