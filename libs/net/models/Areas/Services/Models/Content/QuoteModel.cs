using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// QuoteModel class, provides a model that represents a label to identify content information.
/// </summary>
public class QuoteModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to label.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The statement.
    /// </summary>
    public string Statement { get; set; } = "";

    /// <summary>
    /// get/set - The person attributed to the statement.
    /// </summary>
    public string Byline { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an QuoteModel.
    /// </summary>
    public QuoteModel() { }

    /// <summary>
    /// Creates a new instance of an QuoteModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public QuoteModel(Entities.Quote entity) : base(entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.Statement = entity.Statement;
        this.Byline = entity.Byline;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentModel object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.Quote ToEntity(long contentId)
    {
        var entity = (Entities.Quote)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Quote(QuoteModel model)
    {
        return new Entities.Quote(model.ContentId, model.Statement, model.Byline)
        {
            Id = model.Id,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
