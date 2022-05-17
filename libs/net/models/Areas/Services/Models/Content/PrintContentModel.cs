namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentModel class, provides a model that represents an print content.
/// </summary>
public class PrintContentModel : ContentModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Edition { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string StoryType { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Byline { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an PrintContentModel.
    /// </summary>
    public PrintContentModel() { }

    /// <summary>
    /// Creates a new instance of an PrintContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public PrintContentModel(Entities.PrintContent entity) : base(entity?.Content!)
    {
        this.Edition = entity?.Edition ?? throw new ArgumentNullException(nameof(entity));
        this.Section = entity.Section;
        this.StoryType = entity.StoryType;
        this.Byline = entity.Byline;
    }

    /// <summary>
    /// Creates a new instance of an PrintContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public PrintContentModel(Entities.Content entity) : base(entity)
    {
        if (entity?.PrintContent == null) throw new ArgumentNullException(nameof(entity));

        this.Edition = entity.PrintContent.Edition;
        this.Section = entity.PrintContent.Section;
        this.StoryType = entity.PrintContent.StoryType;
        this.Byline = entity.PrintContent.Byline;
    }
    #endregion
}
