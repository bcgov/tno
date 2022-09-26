using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ContentFilter class, provides a model for searching content.
/// </summary>
public class ContentFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include content with this status.
    /// </summary>
    public ContentStatus? Status { get; set; }

    /// <summary>
    /// get/set - Only include content with this product.
    /// </summary>
    public string? Product { get; set; }

    /// <summary>
    /// get/set - Only include content with this product.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// get/set - Only include content with this type.
    /// </summary>
    public ContentType? ContentType { get; set; }

    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Only include content created or updated by this user.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// get/set - Only include content with this source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - Only include content with this other source.
    /// </summary>
    public string? OtherSource { get; set; }

    /// <summary>
    /// get/set - Only include content with this headline.
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// get/set - Only include content with this page name.
    /// </summary>
    public string? PageName { get; set; }

    /// <summary>
    /// get/set - Only include content created on this date.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - Only include content created on or after this date.
    /// </summary>
    public DateTime? CreatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only included content created on or before this date.
    /// </summary>
    public DateTime? CreatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on this date.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on or after this date.
    /// </summary>
    public DateTime? UpdatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only include content updated on or before this date.
    /// </summary>
    public DateTime? UpdatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on this date.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on or after this date.
    /// </summary>
    public DateTime? PublishedStartOn { get; set; }

    /// <summary>
    /// get/set - Only include content published on or before this date.
    /// </summary>
    public DateTime? PublishedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include content with the section.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set - Only include content with the edition.
    /// </summary>
    public string? Edition { get; set; }

    /// <summary>
    /// get/set - Only include content with the byline.
    /// </summary>
    public string? Byline { get; set; }

    /// <summary>
    /// get/set - Only include content with a category.
    /// </summary>
    public bool? IncludedInCategory { get; set; }

    /// <summary>
    /// get/set - Only include content with the specified actions.
    /// </summary>
    public string[] Actions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - Sort the content in the specified order.
    /// </summary>
    public string[] Sort { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public ContentFilter() { }

    public ContentFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Product = filter.GetStringValue(nameof(this.Product));
        this.OtherSource = filter.GetStringValue(nameof(this.OtherSource));
        this.Headline = filter.GetStringValue(nameof(this.Headline));
        this.Section = filter.GetStringValue(nameof(this.Section));
        this.PageName = filter.GetStringValue(nameof(this.PageName));
        this.Edition = filter.GetStringValue(nameof(this.Edition));
        this.Byline = filter.GetStringValue(nameof(this.Byline));

        this.Status = filter.GetEnumNullValue<ContentStatus>(nameof(this.Status));
        this.ContentType = filter.GetEnumNullValue<ContentType>(nameof(this.ContentType));

        this.IncludedInCategory = filter.GetBoolNullValue(nameof(this.IncludedInCategory));

        this.ProductId = filter.GetIntNullValue(nameof(this.ProductId));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.UserId = filter.GetIntNullValue(nameof(this.UserId));
        this.SourceId = filter.GetIntNullValue(nameof(this.SourceId));

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
