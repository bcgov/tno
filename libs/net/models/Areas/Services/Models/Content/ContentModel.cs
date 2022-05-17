using System.ComponentModel.DataAnnotations;
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
    /// get/set - The workflow status of the content.
    /// </summary>
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.InProgress;

    /// <summary>
    /// get/set - Foreign key to content type.
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    /// get/set - Foreign key to media type.
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Foreign key to license.
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - Foreign key to series.
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - Provides a way to dynamically add new series.
    /// </summary>
    public string? OtherSeries { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who owns the content.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Foreign key to data source.
    /// </summary>
    public int? DataSourceId { get; set; }

    /// <summary>
    /// get/set - The id of the source.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set - The headline.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - A unique identifier for the content from the source.
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - The page.
    /// </summary>
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set - Summary or body of content.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - The transcription.
    /// </summary>
    public string Transcription { get; set; } = "";

    /// <summary>
    /// get/set - The source URL.
    /// </summary>
    public string SourceUrl { get; set; } = "";

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
    /// get - An array of actions.
    /// </summary>
    public IEnumerable<ContentActionModel> Actions { get; set; } = Array.Empty<ContentActionModel>();

    /// <summary>
    /// get - An array of categories.
    /// </summary>
    public IEnumerable<ContentCategoryModel> Categories { get; set; } = Array.Empty<ContentCategoryModel>();

    /// <summary>
    /// get - An array of tags.
    /// </summary>
    public IEnumerable<ContentTagModel> Tags { get; set; } = Array.Empty<ContentTagModel>();

    /// <summary>
    /// get - An array of tone pools.
    /// </summary>
    public IEnumerable<ContentTonePoolModel> TonePools { get; set; } = Array.Empty<ContentTonePoolModel>();

    /// <summary>
    /// get - An array of file references.
    /// </summary>
    public IEnumerable<FileReferenceModel> FileReferences { get; set; } = Array.Empty<FileReferenceModel>();
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
        this.WorkflowStatus = entity.WorkflowStatus;
        this.ContentTypeId = entity.ContentTypeId;
        this.MediaTypeId = entity.MediaTypeId;
        this.LicenseId = entity.LicenseId;
        this.SeriesId = entity.SeriesId;
        this.OwnerId = entity.OwnerId;
        this.DataSourceId = entity.DataSourceId;
        this.Source = entity.Source;
        this.Headline = entity.Headline;
        this.Uid = entity.Uid;
        this.Page = entity.Page;
        this.Summary = entity.Summary;
        this.Transcription = entity.Transcription;
        this.SourceUrl = entity.SourceUrl;
        this.PublishedOn = entity.PublishedOn;

        this.Actions = entity.ActionsManyToMany.Select(e => new ContentActionModel(e));
        this.Categories = entity.CategoriesManyToMany.Select(e => new ContentCategoryModel(e));
        this.Tags = entity.TagsManyToMany.Select(e => new ContentTagModel(e));
        this.TonePools = entity.TonePoolsManyToMany.Select(e => new ContentTonePoolModel(e));
        this.FileReferences = entity.FileReferences.Select(e => new FileReferenceModel(e));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Content(ContentModel model)
    {
        var entity = new Entities.Content(model.Uid, model.Headline, model.Source, model.DataSourceId, model.ContentTypeId, model.MediaTypeId, model.LicenseId, model.OwnerId)
        {
            Id = model.Id,
            Status = model.Status,
            WorkflowStatus = model.WorkflowStatus,
            SeriesId = model.SeriesId,
            Page = model.Page,
            PublishedOn = model.PublishedOn,
            Summary = model.Summary,
            Transcription = model.Transcription,
            SourceUrl = model.SourceUrl,
            Version = model.Version ?? 0,
        };

        if (!String.IsNullOrWhiteSpace(model.OtherSeries))
        {
            entity.Series = new Entities.Series(model.OtherSeries);
        }

        entity.ActionsManyToMany.AddRange(model.Actions.Select(a => a.ToEntity(entity.Id)));
        entity.CategoriesManyToMany.AddRange(model.Categories.Select(c => c.ToEntity(entity.Id)));
        entity.TagsManyToMany.AddRange(model.Tags.Select(t => t.ToEntity(entity.Id)));
        entity.TonePoolsManyToMany.AddRange(model.TonePools.Select(tp => tp.ToEntity(entity.Id)));
        entity.FileReferences.AddRange(model.FileReferences.Select(f => f.ToEntity(entity.Id)));

        return entity;
    }
    #endregion
}
