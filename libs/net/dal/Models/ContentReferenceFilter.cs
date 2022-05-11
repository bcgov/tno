using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Models;

public class ContentReferenceFilter : PageFilter
{
    #region Properties
    public WorkflowStatus? WorkflowStatus { get; set; }
    public int? Offset { get; set; }
    public int? Partition { get; set; }
    public string? Source { get; set; }
    public string? Uid { get; set; }
    public string? Topic { get; set; }
    public DateTime? PublishedOn { get; set; }
    public DateTime? PublishedStartOn { get; set; }
    public DateTime? PublishedEndOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? UpdatedStartOn { get; set; }
    public DateTime? UpdatedEndOn { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public ContentReferenceFilter() { }

    public ContentReferenceFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Source = filter.GetStringValue(nameof(this.Source));
        this.Uid = filter.GetStringValue(nameof(this.Uid));
        this.Topic = filter.GetStringValue(nameof(this.Topic));

        this.Offset = filter.GetIntNullValue(nameof(this.Offset));
        this.Partition = filter.GetIntNullValue(nameof(this.Partition));

        this.PublishedOn = filter.GetDateTimeNullValue(nameof(this.PublishedOn));
        this.PublishedStartOn = filter.GetDateTimeNullValue(nameof(this.PublishedStartOn));
        this.PublishedEndOn = filter.GetDateTimeNullValue(nameof(this.PublishedEndOn));

        this.UpdatedOn = filter.GetDateTimeNullValue(nameof(this.UpdatedOn));
        this.UpdatedStartOn = filter.GetDateTimeNullValue(nameof(this.UpdatedStartOn));
        this.UpdatedEndOn = filter.GetDateTimeNullValue(nameof(this.UpdatedEndOn));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
