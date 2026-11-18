using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Folder;

/// <summary>
/// ContentModel class, provides a model that represents content.
/// </summary>
public class ContentModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content.
    /// </summary>
    public long Id { get; set; } = default!;

    /// <summary>
    /// get/set - headline of content.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - The byline.
    /// </summary>
    public string Byline { get; set; } = "";

    /// <summary>
    /// get/set - The summary.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - Story body text.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Other source code name.
    /// </summary>
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - The edition.
    /// </summary>
    public string Edition { get; set; } = "";

    /// <summary>
    /// get/set - The section.
    /// </summary>
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The page.
    /// </summary>
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set - When content was posted to MMI.
    /// </summary>
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - When content was published on.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public SortableModel<int>? Source { get; set; }

    /// <summary>
    /// get/set - The media type.
    /// </summary>
    public SortableModel<int>? MediaType { get; set; }

    /// <summary>
    /// get/set - The series.
    /// </summary>
    public SortableModel<int>? Series { get; set; }

    /// <summary>
    /// get/set - The contributor.
    /// </summary>
    public SortableModel<int>? Contributor { get; set; }

    /// <summary>
    /// get/set - The owner of this content.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - Private content is not searchable.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// get/set - An array of tone pools.
    /// </summary>
    public IEnumerable<TonePoolModel> TonePools { get; set; } = Array.Empty<TonePoolModel>();

    /// <summary>
    /// get - An array of topics and scores.
    /// </summary>
    public IEnumerable<FolderContentTopicModel> Topics { get; set; } = Array.Empty<FolderContentTopicModel>();

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
    public ContentModel(Entities.Content entity)
    {
        this.Id = entity.Id;
        this.Headline = entity.Headline;
        this.Byline = entity.Byline;
        this.Summary = entity.Summary;
        this.Body = entity.Body;
        this.OtherSource = entity.OtherSource;
        this.Section = entity.Section;
        this.Edition = entity.Edition;
        this.Page = entity.Page;
        this.PostedOn = entity.PostedOn;
        this.PublishedOn = entity.PublishedOn;
        this.IsPrivate = entity.IsPrivate;
        this.Source = entity.Source != null ? new SortableModel<int>(entity.Source) : null;
        this.MediaType = entity.MediaType != null ? new SortableModel<int>(entity.MediaType) : null;
        this.Series = entity.Series != null ? new SortableModel<int>(entity.Series) : null;
        this.Contributor = entity.Contributor != null ? new SortableModel<int>(entity.Contributor) : null;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.TonePools = entity.TonePoolsManyToMany.Select(tp => new TonePoolModel(tp));
        this.Topics = entity.TopicsManyToMany.Select(e => new FolderContentTopicModel(e));
    }
    #endregion
}
