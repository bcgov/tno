using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentModel class, provides a model that represents an content.
/// </summary>
public class ContentModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The status of the content.
    /// </summary>
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    /// <summary>
    /// get/set - The type of content and form to use.
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The data source.
    /// </summary>
    public SourceModel? Source { get; set; }

    /// <summary>
    /// get/set - The id of the source.
    /// </summary>
    [MaxLength(100)]
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to media type.
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The media type.
    /// </summary>
    public MediaTypeModel? MediaType { get; set; }

    /// <summary>
    /// get/set - Foreign key to license.
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - Foreign key to series (show/program).
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The series (show/program).
    /// </summary>
    public SeriesModel? Series { get; set; }

    /// <summary>
    /// get/set - Provides a way to dynamically add new series.
    /// </summary>
    [MaxLength(100)]
    public string? OtherSeries { get; set; }

    /// <summary>
    /// get/set - Foreign key to contributor.
    /// </summary>
    public int? ContributorId { get; set; }

    /// <summary>
    /// get/set - The contributor.
    /// </summary>
    public ContributorModel? Contributor { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who owns the content.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The content type.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - The headline.
    /// </summary>
    [MaxLength(500)]
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - The author or writer's name.
    /// </summary>
    [MaxLength(500)]
    public string Byline { get; set; } = "";

    /// <summary>
    /// get/set - Unique identifier within MMI based on a hash of values.
    /// </summary>
    [MaxLength(500)]
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - Unique identifier from the external source if provided.
    /// </summary>
    [MaxLength(500)]
    public string ExternalUid { get; set; } = "";

    /// <summary>
    /// get/set - The print content edition.
    /// </summary>
    [MaxLength(100)]
    public string Edition { get; set; } = "";

    /// <summary>
    /// get/set - The print content section.
    /// </summary>
    [MaxLength(100)]
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The print content page.
    /// </summary>
    [MaxLength(10)]
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set - The story summary or abstract.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - The story body.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - The source URL.
    /// </summary>
    [MaxLength(500)]
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// get/set - When an editor posted the content.
    /// </summary>
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - When the content has been or will be published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - Whether content is hidden from search results.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// get/set - Whether the content has been approved for publishing.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// get/set - Private content is not searchable.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// get/set - The first file reference's image content if available.
    /// </summary>
    public string? ImageContent { get; set; }

    /// <summary>
    /// get/set - Upload files with content.
    /// </summary>
    [DataType(DataType.Upload)]
    public List<IFormFile>? Files { get; set; }

    /// <summary>
    /// get - Dictionary of versions associated with this content.
    /// This provides subscribers the ability to customize the content.
    /// The key is the user's ID.
    /// </summary>
    public Dictionary<int, Entities.Models.ContentVersion> Versions { get; set; } = new();

    /// <summary>
    /// get/set - An array of actions.
    /// </summary>
    public IEnumerable<ContentActionModel> Actions { get; set; } = Array.Empty<ContentActionModel>();

    /// <summary>
    /// get/set - An array of categories.
    /// </summary>
    public IEnumerable<ContentTopicModel> Topics { get; set; } = Array.Empty<ContentTopicModel>();

    /// <summary>
    /// get/set - An array of tags.
    /// </summary>
    public IEnumerable<ContentTagModel> Tags { get; set; } = Array.Empty<ContentTagModel>();

    /// <summary>
    /// get/set - An array of time tracking entries.
    /// </summary>
    public IEnumerable<TimeTrackingModel> TimeTrackings { get; set; } = Array.Empty<TimeTrackingModel>();

    /// <summary>
    /// get/set - An array of labels.
    /// </summary>
    public IEnumerable<ContentLabelModel> Labels { get; set; } = Array.Empty<ContentLabelModel>();

    /// <summary>
    /// get/set - An array of tone pools.
    /// </summary>
    public IEnumerable<ContentTonePoolModel> TonePools { get; set; } = Array.Empty<ContentTonePoolModel>();

    /// <summary>
    /// get/set - An array of file references.
    /// </summary>
    public IEnumerable<FileReferenceModel> FileReferences { get; set; } = Array.Empty<FileReferenceModel>();

    /// <summary>
    /// get/set - An array of notification instances.
    /// </summary>
    public IEnumerable<NotificationInstanceModel> Notifications { get; set; } = Array.Empty<NotificationInstanceModel>();

    /// <summary>
    /// get - Collection of users who want to be notified about this content.
    /// </summary>
    public IEnumerable<UserContentNotificationModel> UserNotifications { get; set; } = Array.Empty<UserContentNotificationModel>();

    /// <summary>
    /// get/set - An array of quotes.
    /// </summary>
    public IEnumerable<QuoteModel> Quotes { get; set; } = Array.Empty<QuoteModel>();

    /// <summary>
    /// get/set - Whether content is CBRA unqualified.
    /// </summary>
    public bool IsCBRAUnqualified { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentModel.
    /// </summary>
    public ContentModel() { }

    /// <summary>
    /// Creates a new instance of an ContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ContentModel(Entities.Content entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity?.Id ?? throw new ArgumentNullException(nameof(entity));
        this.Status = entity.Status;
        this.ContentType = entity.ContentType;
        this.SourceId = entity.SourceId;
        this.Source = entity.Source != null ? new SourceModel(entity.Source) : null;
        this.OtherSource = entity.OtherSource;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType != null ? new MediaTypeModel(entity.MediaType, options) : null;
        this.LicenseId = entity.LicenseId;
        this.SeriesId = entity.SeriesId;
        this.Series = entity.Series != null ? new SeriesModel(entity.Series) : null;
        this.ContributorId = entity.ContributorId;
        this.Contributor = entity.Contributor != null ? new ContributorModel(entity.Contributor) : null;
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Headline = entity.Headline;
        this.Byline = entity.Byline;
        this.Uid = entity.Uid;
        this.ExternalUid = entity.ExternalUid;
        this.Edition = entity.Edition;
        this.Section = entity.Section;
        this.Page = entity.Page;
        this.Summary = entity.Summary;
        this.Body = entity.Body;
        this.SourceUrl = entity.SourceUrl;
        this.PostedOn = entity.PostedOn;
        this.PublishedOn = entity.PublishedOn;
        this.IsHidden = entity.IsHidden;
        this.IsApproved = entity.IsApproved;
        this.IsPrivate = entity.IsPrivate;

        this.Actions = entity.ActionsManyToMany.Select(e => new ContentActionModel(e));
        this.Topics = entity.TopicsManyToMany.Select(e => new ContentTopicModel(e));
        this.Tags = entity.TagsManyToMany.Select(e => new ContentTagModel(e));
        this.TimeTrackings = entity.TimeTrackings.Select(e => new TimeTrackingModel(e));
        this.Labels = entity.Labels.Select(e => new ContentLabelModel(e));
        this.TonePools = entity.TonePoolsManyToMany.Select(e => new ContentTonePoolModel(e));
        this.FileReferences = entity.FileReferences.Select(e => new FileReferenceModel(e));
        this.Quotes = entity.Quotes.Select(e => new QuoteModel(e));
        this.UserNotifications = entity.UserNotifications.Select(un => new UserContentNotificationModel(un));
        this.Versions = entity.Versions;
        this.IsCBRAUnqualified = entity.IsCBRAUnqualified;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Content(ContentModel model)
    {
        var entity = new Entities.Content(model.Uid, model.Headline, model.OtherSource, model.SourceId, model.ContentType, model.LicenseId, model.MediaTypeId, model.OwnerId)
        {
            Id = model.Id,
            Status = model.Status,
            SeriesId = model.SeriesId,
            ContributorId = model.ContributorId,
            Byline = model.Byline,
            Edition = model.Edition,
            Section = model.Section,
            Page = model.Page,
            PostedOn = model.PostedOn,
            PublishedOn = model.PublishedOn,
            Summary = model.Summary,
            Body = model.Body,
            SourceUrl = model.SourceUrl,
            IsHidden = model.IsHidden,
            IsApproved = model.IsApproved,
            IsPrivate = model.IsPrivate,
            Versions = model.Versions,
            Version = model.Version ?? 0,
        };

        if (!String.IsNullOrWhiteSpace(model.OtherSeries))
        {
            entity.Series = new Entities.Series(model.OtherSeries, true, model.SourceId);
        }

        entity.ActionsManyToMany.AddRange(model.Actions.Select(a => a.ToEntity(entity.Id)));
        entity.TopicsManyToMany.AddRange(model.Topics.Select(c => c.ToEntity(entity.Id)));
        entity.TagsManyToMany.AddRange(model.Tags.Select(t => t.ToEntity(entity.Id)));
        entity.TimeTrackings.AddRange(model.TimeTrackings.Select(t => t.ToEntity(entity.Id)));
        entity.Labels.AddRange(model.Labels.Select(f => f.ToEntity(entity.Id)));
        entity.TonePoolsManyToMany.AddRange(model.TonePools.Select(tp => tp.ToEntity(entity.Id)));
        entity.FileReferences.AddRange(model.FileReferences.Select(f => f.ToEntity(entity.Id)));
        entity.NotificationsManyToMany.AddRange(model.Notifications.Select(n => (Entities.NotificationInstance)n));
        entity.Quotes.AddRange(model.Quotes.Select(f => f.ToEntity(entity.Id)));
        entity.IsCBRAUnqualified = model.IsCBRAUnqualified;

        return entity;
    }

    public Entities.Content ToContent()
    {
        var content = (Entities.Content)this;
        content.Source = this.Source != null ? (Entities.Source)this.Source : null;
        content.Series = this.Series != null ? (Entities.Series)this.Series : null;
        content.MediaType = this.MediaType != null ? (Entities.MediaType)this.MediaType : null;
        content.Contributor = this.Contributor != null ? (Entities.Contributor)this.Contributor : null;
        return content;
    }
    #endregion
}
