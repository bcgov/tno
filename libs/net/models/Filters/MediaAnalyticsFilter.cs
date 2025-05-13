using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
namespace TNO.Models.Filters;

public class MediaAnalyticsFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - If you want to include the specific user in the results.
    /// </summary>
    public int? SourceId { get; set; }
    public int? MediaTypeId { get; set; }
    
    #endregion

    #region Constructors
    public MediaAnalyticsFilter() { }

    public MediaAnalyticsFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.SourceId = filter.GetIntNullValue(nameof(this.SourceId));
        
        this.MediaTypeId = filter.GetIntNullValue(nameof(this.MediaTypeId));
       
        
    }
    #endregion
}
