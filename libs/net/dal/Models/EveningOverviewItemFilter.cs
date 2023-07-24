using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

public class EveningOverviewItemFilter : PageFilter
{
    #region Properties
    public int? SourceId { get; set; }
    public string? Time { get; set; }
    public int? SeriesId { get; set; }
    #endregion

    #region Constructors
    public EveningOverviewItemFilter() { }

    public EveningOverviewItemFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Time = filter.GetStringValue(nameof(this.Time));
        this.SourceId = filter.GetIntNullValue(nameof(this.SourceId));
        this.SeriesId = filter.GetIntNullValue(nameof(this.SeriesId));
    }
    #endregion
}
