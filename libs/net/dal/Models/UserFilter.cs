using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
namespace TNO.DAL.Models;

public class UserFilter : PageFilter
{
    #region Properties
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public bool? IsEnabled { get; set; }
    public Entities.UserStatus? Status { get; set; }
    public bool? IsSystemAccount { get; set; }
    public string[] Sort { get; set; } = Array.Empty<string>();
    public string? Keyword { get; set; }
    public string? RoleName { get; set; }

    #endregion

    #region Constructors
    public UserFilter() { }

    public UserFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Username = filter.GetStringValue(nameof(this.Username));
        this.Email = filter.GetStringValue(nameof(this.Email));
        this.FirstName = filter.GetStringValue(nameof(this.FirstName));
        this.LastName = filter.GetStringValue(nameof(this.LastName));
        this.Name = filter.GetStringValue(nameof(this.Name));
        this.IsEnabled = filter.GetBoolNullValue(nameof(this.IsEnabled));
        this.Status = filter.GetEnumNullValue<Entities.UserStatus>(nameof(this.Status));
        this.IsSystemAccount = filter.GetBoolNullValue(nameof(this.IsSystemAccount));
        this.Keyword = filter.GetStringValue(nameof(this.Keyword));
        this.RoleName = filter.GetStringValue(nameof(this.RoleName));

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
