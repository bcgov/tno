namespace TNO.API.Models.SignalR;

/// <summary>
/// ContentMessageModel class, provides a model that represents a signalR clip message.
/// </summary>
public class ContentMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The type of content.
    /// </summary>
    public Entities.ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - The status of the content.
    /// </summary>
    public Entities.ContentStatus Status { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user requested this content.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Content headline.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to media type.
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Foreign key to data source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - A source that isn't linked or maintained in a list.
    /// </summary>
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to license.
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - Foreign key to series (show/program).
    /// </summary>
    public int? SeriesId { get; set; }

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
    /// get/set - Whether the content has been approved for publishing.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// get/set - When an editor posted the content.
    /// </summary>
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - When the content has been or will be published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get - An array of file references.
    /// </summary>
    public IEnumerable<FileReferenceModel> FileReferences { get; set; } = Array.Empty<FileReferenceModel>();

    /// <summary>
    /// get - An array of actions.
    /// </summary>
    public IEnumerable<ContentActionModel> Actions { get; set; } = Array.Empty<ContentActionModel>();

    /// <summary>
    /// get/set - An array of tone pools.
    /// </summary>
    public IEnumerable<ContentTonePoolModel> TonePools { get; set; } = Array.Empty<ContentTonePoolModel>();

    /// <summary>
    /// get/set - Version number.
    /// </summary>
    public long? Version { get; set; }

    /// <summary>
    /// get/set - The reason the message has been sent.
    /// The app can then act based on the reason.
    /// </summary>
    public string? Reason { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentMessageModel.
    /// </summary>
    public ContentMessageModel() { }

    /// <summary>
    /// Creates a new instance of an ContentMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="reason"></param>
    public ContentMessageModel(Entities.Content entity, string? reason = null)
    {
        this.Id = entity.Id;
        this.ContentType = entity.ContentType;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.LicenseId = entity.LicenseId;
        this.MediaTypeId = entity.MediaTypeId;
        this.Status = entity.Status;
        this.OwnerId = entity.OwnerId;
        this.Uid = entity.Uid;
        this.Headline = entity.Headline;
        this.Byline = entity.Byline;
        this.Edition = entity.Edition;
        this.Section = entity.Section;
        this.Page = entity.Page;
        this.IsApproved = entity.IsApproved;
        this.PostedOn = entity.PostedOn;
        this.PublishedOn = entity.PublishedOn;
        this.FileReferences = entity.FileReferences.Select(fr => new FileReferenceModel(fr));
        this.TonePools = entity.TonePoolsManyToMany.Select(tp => new ContentTonePoolModel(tp));
        this.Actions = entity.ActionsManyToMany.Select(a => new ContentActionModel(a));
        this.Version = entity.Version;
        this.Reason = reason;
    }

    /// <summary>
    /// Creates a new instance of an ContentMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="reason"></param>
    public ContentMessageModel(Areas.Services.Models.Content.ContentModel model, string? reason = null)
    {
        this.Id = model.Id;
        this.ContentType = model.ContentType;
        this.SourceId = model.SourceId;
        this.OtherSource = model.OtherSource;
        this.SeriesId = model.SeriesId;
        this.LicenseId = model.LicenseId;
        this.MediaTypeId = model.MediaTypeId;
        this.Status = model.Status;
        this.OwnerId = model.OwnerId;
        this.Uid = model.Uid;
        this.Headline = model.Headline;
        this.Byline = model.Byline;
        this.Edition = model.Edition;
        this.Section = model.Section;
        this.Page = model.Page;
        this.IsApproved = model.IsApproved;
        this.PostedOn = model.PostedOn;
        this.PublishedOn = model.PublishedOn;
        this.FileReferences = model.FileReferences.Select(fr => new FileReferenceModel(fr));
        this.TonePools = model.TonePools.Select(tp => new ContentTonePoolModel(tp));
        this.Actions = model.Actions.Select(a => new ContentActionModel(a));
        this.Version = model.Version;
        this.Reason = reason;
    }
    #endregion
}
