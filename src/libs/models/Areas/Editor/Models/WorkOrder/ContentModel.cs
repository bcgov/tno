namespace TNO.API.Areas.Editor.Models.WorkOrder;

/// <summary>
/// ContentModel class, provides a model that represents an user.
/// </summary>
public class ContentModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public long Id { get; set; } = default!;

    /// <summary>
    /// get/set - headline of content.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - Other source or the source code.
    /// </summary>
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - Whether the transcription has been approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// get/set - The media type name.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the media type.
    /// </summary>
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The series name.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// get/set - Foreign key to the series.
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The contributor name.
    /// </summary>
    public string? Contributor { get; set; }
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
        this.OtherSource = entity.OtherSource;
        this.IsApproved = entity.IsApproved;
        this.MediaType = entity.MediaType?.Name;
        this.Series = entity.Series?.Name;
        this.Contributor = entity.Contributor?.Name;
    }
    #endregion
}
