namespace TNO.API.Areas.Subscriber.Models.Content;

/// <summary>
/// ContentVersionModel class, provides a model that represents an content version.
/// </summary>
public class ContentVersionModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user who owns the content.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - The headline.
    /// </summary>
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - The author or writer's name.
    /// </summary>
    public string Byline { get; set; } = "";

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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentVersionModel.
    /// </summary>
    public ContentVersionModel() { }

    /// <summary>
    /// Creates a new instance of an ContentVersionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentVersionModel(Entities.Models.ContentVersion entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Headline = entity.Headline;
        this.Byline = entity.Byline;
        this.Edition = entity.Edition;
        this.Section = entity.Section;
        this.Page = entity.Page;
        this.Summary = entity.Summary;
        this.Body = entity.Body;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Models.ContentVersion(ContentVersionModel model)
    {
        var entity = new Entities.Models.ContentVersion(model.OwnerId)
        {
            Headline = model.Headline,
            Byline = model.Byline,
            Edition = model.Edition,
            Section = model.Section,
            Page = model.Page,
            Summary = model.Summary,
            Body = model.Body
        };

        return entity;
    }
    #endregion
}
