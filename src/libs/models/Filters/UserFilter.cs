using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
namespace TNO.Models.Filters;

public class UserFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - If you want to include the specific user in the results.
    /// </summary>
    public int? IncludeUserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public bool? IsEnabled { get; set; }
    public int? IsSubscribedToProductId { get; set; }
    public int? IsSubscribedToReportId { get; set; }
    public int? IsSubscribedToNotificationId { get; set; }
    public Entities.AVOverviewTemplateType? IsSubscribedToEveningOverview { get; set; }
    public Entities.UserStatus? Status { get; set; }
    public bool? IsSystemAccount { get; set; }
    public Entities.UserAccountType[] AccountTypes { get; set; } = Array.Empty<Entities.UserAccountType>();
    public string? Keyword { get; set; }
    public string? RoleName { get; set; }

    #endregion

    #region Constructors
    public UserFilter() { }

    public UserFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.IncludeUserId = filter.GetIntNullValue(nameof(this.IncludeUserId));
        this.Username = filter.GetStringValue(nameof(this.Username));
        this.Email = filter.GetStringValue(nameof(this.Email));
        this.FirstName = filter.GetStringValue(nameof(this.FirstName));
        this.LastName = filter.GetStringValue(nameof(this.LastName));
        this.Name = filter.GetStringValue(nameof(this.Name));
        this.IsEnabled = filter.GetBoolNullValue(nameof(this.IsEnabled));
        this.IsSubscribedToProductId = filter.GetIntNullValue(nameof(this.IsSubscribedToProductId));
        this.IsSubscribedToReportId = filter.GetIntNullValue(nameof(this.IsSubscribedToReportId));
        this.IsSubscribedToNotificationId = filter.GetIntNullValue(nameof(this.IsSubscribedToNotificationId));
        this.IsSubscribedToEveningOverview = filter.GetEnumNullValue<Entities.AVOverviewTemplateType>(nameof(this.IsSubscribedToEveningOverview));
        this.Status = filter.GetEnumNullValue<Entities.UserStatus>(nameof(this.Status));
        this.IsSystemAccount = filter.GetBoolNullValue(nameof(this.IsSystemAccount));
        this.AccountTypes = filter.GetEnumArrayValue<Entities.UserAccountType>(nameof(this.AccountTypes));
        this.Keyword = filter.GetStringValue(nameof(this.Keyword));
        this.RoleName = filter.GetStringValue(nameof(this.RoleName));
    }
    #endregion
}
