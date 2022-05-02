using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Models;

public class ContentFilter : PageFilter
{
    #region Properties
    public ContentStatus? State { get; set; }
    public WorkflowStatus? WorkflowStatus { get; set; }
    public int? ContentTypeId { get; set; }
    public int? MediaTypeId { get; set; }
    public string? MediaType { get; set; }
    public int? OwnerId { get; set; }
    public int? UserId { get; set; }
    public int? DataSourceId { get; set; }
    public string? Source { get; set; }
    public string? Headline { get; set; }
    public string? PageName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? CreatedStartOn { get; set; }
    public DateTime? CreatedEndOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? UpdatedStartOn { get; set; }
    public DateTime? UpdatedEndOn { get; set; }
    public DateTime? PublishedOn { get; set; }
    public DateTime? PublishedStartOn { get; set; }
    public DateTime? PublishedEndOn { get; set; }
    public string? Section { get; set; }
    public string? Edition { get; set; }
    public string? StoryType { get; set; }
    public string? Byline { get; set; }
    public string[] Actions { get; set; } = Array.Empty<string>();
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public ContentFilter() { }

    public ContentFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Source = filter.GetStringValue(nameof(this.Source));
        this.Headline = filter.GetStringValue(nameof(this.Headline));
        this.Section = filter.GetStringValue(nameof(this.Section));
        this.PageName = filter.GetStringValue(nameof(this.PageName));
        this.Edition = filter.GetStringValue(nameof(this.Edition));
        this.StoryType = filter.GetStringValue(nameof(this.StoryType));
        this.Byline = filter.GetStringValue(nameof(this.Byline));
        this.MediaType = filter.GetStringValue(nameof(this.MediaType));

        this.ContentTypeId = filter.GetIntNullValue(nameof(this.ContentTypeId));
        this.MediaTypeId = filter.GetIntNullValue(nameof(this.MediaTypeId));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.UserId = filter.GetIntNullValue(nameof(this.UserId));
        this.DataSourceId = filter.GetIntNullValue(nameof(this.DataSourceId));

        this.CreatedOn = filter.GetDateTimeNullValue(nameof(this.CreatedOn));
        this.CreatedStartOn = filter.GetDateTimeNullValue(nameof(this.CreatedStartOn));
        this.CreatedEndOn = filter.GetDateTimeNullValue(nameof(this.CreatedEndOn));
        this.UpdatedOn = filter.GetDateTimeNullValue(nameof(this.UpdatedOn));
        this.UpdatedStartOn = filter.GetDateTimeNullValue(nameof(this.UpdatedStartOn));
        this.UpdatedEndOn = filter.GetDateTimeNullValue(nameof(this.UpdatedEndOn));
        this.PublishedOn = filter.GetDateTimeNullValue(nameof(this.PublishedOn));
        this.PublishedStartOn = filter.GetDateTimeNullValue(nameof(this.PublishedStartOn));
        this.PublishedEndOn = filter.GetDateTimeNullValue(nameof(this.PublishedEndOn));

        this.Actions = filter.GetStringArrayValue(nameof(this.Actions));
        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
