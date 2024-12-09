using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TNO.API.Areas.Subscriber.Content;
using TNO.API.Models;

using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.Content;

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
    public ContentType ContentType { get; set; } = ContentType.AudioVideo;

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
    /// get/set - The content type.
    /// </summary>
    public LicenseModel? License { get; set; }

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
    /// get/set - Foreign key to data source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The data source.
    /// </summary>
    public SourceModel? Source { get; set; }

    /// <summary>
    /// get/set - A source that isn't linked or maintained in a list.
    /// </summary>
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - The headline.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - The author or writer's name.
    /// </summary>
    public string Byline { get; set; } = "";

    /// <summary>
    /// get/set - A unique identifier for the content from the source.
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - The print content edition.
    /// </summary>
    public string Edition { get; set; } = "";

    /// <summary>
    /// get/set - The print content section.
    /// </summary>
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The print content page.
    /// </summary>
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
    /// get - An array of file references.
    /// </summary>
    public IEnumerable<FileReferenceModel> FileReferences { get; set; } = Array.Empty<FileReferenceModel>();

    /// <summary>
    /// get/set - The source URL.
    /// </summary>
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// get/set - Whether this content has been hidden.
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
    /// get/set - When an editor posted the content.
    /// </summary>
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - When the content has been or will be published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

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
    /// get - An array of actions.
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
    /// get/set - An array of labels.
    /// </summary>
    public IEnumerable<ContentLabelModel> Labels { get; set; } = Array.Empty<ContentLabelModel>();

    /// <summary>
    /// get/set - An array of tone pools.
    /// </summary>
    public IEnumerable<ContentTonePoolModel> TonePools { get; set; } = Array.Empty<ContentTonePoolModel>();

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
    public ContentModel(Entities.Content entity) : base(entity)
    {
        this.Id = entity?.Id ?? throw new ArgumentNullException(nameof(entity));
        this.Status = entity.Status;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType != null ? new MediaTypeModel(entity.MediaType) : null;
        this.ContentType = entity.ContentType;
        this.LicenseId = entity.LicenseId;
        this.License = entity.License != null ? new LicenseModel(entity.License) : null;
        this.SeriesId = entity.SeriesId;
        this.Series = entity.Series != null ? new SeriesModel(entity.Series) : null;
        this.ContributorId = entity.ContributorId;
        this.Contributor = entity.Contributor != null ? new ContributorModel(entity.Contributor) : null;
        this.OwnerId = entity.OwnerId;
        this.SourceId = entity.SourceId;
        this.Source = entity.Source != null ? new SourceModel(entity.Source) : null;
        this.OtherSource = entity.OtherSource;
        this.Headline = entity.Headline;
        this.Byline = entity.Byline;
        this.Uid = entity.Uid;
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
        this.FileReferences = entity.FileReferences.Select(e => new FileReferenceModel(e));
        this.Labels = entity.Labels.Select(e => new ContentLabelModel(e));
        this.TonePools = entity.TonePoolsManyToMany.Select(e => new ContentTonePoolModel(e));
        this.Quotes = entity.Quotes.Select(e => new QuoteModel(e));
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
        entity.FileReferences.AddRange(model.FileReferences.Select(f => f.ToEntity(entity.Id)));
        entity.Labels.AddRange(model.Labels.Select(f => f.ToEntity(entity.Id)));
        entity.TonePoolsManyToMany.AddRange(model.TonePools.Select(tp => tp.ToEntity(entity.Id)));
        entity.Quotes.AddRange(model.Quotes.Select(f => f.ToEntity(entity.Id)));
        entity.IsCBRAUnqualified = model.IsCBRAUnqualified;

        return entity;
    }
    #endregion
}
