using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

public class DataSourceFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Topic { get; set; }
    public int? DataLocationId { get; set; }
    public int? MediaTypeId { get; set; }
    public int? LicenseId { get; set; }
    public string[] Actions { get; set; } = Array.Empty<string>();
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public DataSourceFilter() { }

    public DataSourceFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Code = filter.GetStringValue(nameof(this.Code));
        this.Topic = filter.GetStringValue(nameof(this.Topic));

        this.DataLocationId = filter.GetIntNullValue(nameof(this.DataLocationId));
        this.MediaTypeId = filter.GetIntNullValue(nameof(this.MediaTypeId));
        this.LicenseId = filter.GetIntNullValue(nameof(this.LicenseId));

        this.Actions = filter.GetStringArrayValue(nameof(this.Actions));
        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
