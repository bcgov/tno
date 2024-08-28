namespace TNO.Entities.Models;

/// <summary>
/// ContentVersion class, provides a way to store versions of content.
/// </summary>
public class ContentVersion
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the owner.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - Story headline.
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
    /// get/set - The section in the print content.
    /// </summary>
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The page this story was found one.
    /// </summary>
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set - Story summary or abstract text.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - Story body text.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - The source URL in the content.
    /// </summary>
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// get/set - Defines if content is private.
    /// </summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>
    /// get/set - Tone for the content
    /// </summary>
    public int? Tone {get;set;}

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentVersion object.
    /// </summary>
    public ContentVersion() { }

    /// <summary>
    /// Creates a new ContentVersion object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="owner"></param>
    public ContentVersion(Content content, User owner)
    {
        this.OwnerId = owner.Id;
        this.Headline = content.Headline;
        this.Byline = content.Byline;
        this.Edition = content.Edition;
        this.Section = content.Section;
        this.Page = content.Page;
        this.Summary = content.Summary;
        this.Body = content.Body;
        this.SourceUrl = content.SourceUrl;
        this.IsPrivate = content.IsPrivate;
        this.Tone = content.TonePools.Count > 0 ? (int?)content.TonePools[0].Id : null;
    }

    /// <summary>
    /// Creates a new instance of a ContentVersion object, initializes with specified parameters.
    /// </summary>
    /// <param name="ownerId"></param>
    public ContentVersion(int ownerId)
    {
        this.OwnerId = ownerId;
    }
    #endregion
}
