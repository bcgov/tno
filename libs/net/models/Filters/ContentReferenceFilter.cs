using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.Models.Filters;

public class ContentReferenceFilter : PageFilter
{
    #region Properties
    public WorkflowStatus? Status { get; set; }
    public string[]? Sources { get; set; }
    public int[]? MediaTypeIds { get; set; }
    public string? Uid { get; set; }
    public string? Topic { get; set; }
    public DateTime? PublishedOn { get; set; }
    public DateTime? PublishedStartOn { get; set; }
    public DateTime? PublishedEndOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? UpdatedStartOn { get; set; }
    public DateTime? UpdatedEndOn { get; set; }
    #endregion

    #region Constructors
    public ContentReferenceFilter() { }

    public ContentReferenceFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Status = filter.GetEnumNullValue<WorkflowStatus>(nameof(this.Status));

        this.Sources = filter.GetStringArrayValue(nameof(this.Sources));
        this.MediaTypeIds = filter.GetIntArrayValue(nameof(this.MediaTypeIds));
        this.Uid = filter.GetStringValue(nameof(this.Uid));
        this.Topic = filter.GetStringValue(nameof(this.Topic));

        this.PublishedOn = filter.GetDateTimeNullValue(nameof(this.PublishedOn));
        this.PublishedStartOn = filter.GetDateTimeNullValue(nameof(this.PublishedStartOn));
        this.PublishedEndOn = filter.GetDateTimeNullValue(nameof(this.PublishedEndOn));

        this.UpdatedOn = filter.GetDateTimeNullValue(nameof(this.UpdatedOn));
        this.UpdatedStartOn = filter.GetDateTimeNullValue(nameof(this.UpdatedStartOn));
        this.UpdatedEndOn = filter.GetDateTimeNullValue(nameof(this.UpdatedEndOn));
    }
    #endregion
}
