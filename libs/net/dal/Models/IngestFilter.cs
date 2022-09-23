using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.DAL.Models;

public class IngestFilter : PageFilter
{
    #region Properties
    public string? Name { get; set; }
    public string? Topic { get; set; }
    public int? IngestTypeId { get; set; }
    public int? SourceId { get; set; }
    public int? ProductId { get; set; }
    public int? SourceConnectionId { get; set; }
    public int? DestinationConnectionId { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public IngestFilter() { }

    public IngestFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Topic = filter.GetStringValue(nameof(this.Topic));

        this.IngestTypeId = filter.GetIntNullValue(nameof(this.IngestTypeId));
        this.SourceId = filter.GetIntNullValue(nameof(this.SourceId));
        this.ProductId = filter.GetIntNullValue(nameof(this.ProductId));
        this.SourceConnectionId = filter.GetIntNullValue(nameof(this.SourceConnectionId));
        this.DestinationConnectionId = filter.GetIntNullValue(nameof(this.DestinationConnectionId));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
