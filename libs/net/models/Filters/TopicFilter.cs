using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.Models.Filters;

public class TopicFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    /// <summary>
    /// get/set - The type of topic (issue, proactive).
    /// </summary>
    public TopicType? TopicType { get; set; }
    #endregion

    #region Constructors
    public TopicFilter() { }

    public TopicFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Description = filter.GetStringValue(nameof(this.Description));

        this.TopicType = filter.GetEnumNullValue<TopicType>(nameof(this.TopicType));
    }
    #endregion
}
