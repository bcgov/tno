using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// Quote class, provides an entity model to manage work order requests.
/// </summary>
[Table("quote")]
public class Quote : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key, identity seed.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - The statement made by the person.
    /// </summary>
    [Column("statement")]
    public string Statement { get; set; } = "";

    /// <summary>
    /// get/set - The person that the quote is attributed to.
    /// </summary>
    [Column("byline")]
    public string Byline { get; set; } = "";

    /// <summary>
    /// get/set - Is this a relevant quote, one that adds value to content.
    /// </summary>
    [Column("IsRelevant")]
    public bool IsRelevant { get; set; } = true;

    /// <summary>
    /// get/set - Foreign key to the owning content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Content that owns this file reference.
    /// </summary>
    public virtual Content? Content { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Quote object.
    /// </summary>
    protected Quote() { }

    /// <summary>
    /// Creates a new instance of a Quote object, initializes with specified parameters.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="byline"></param>
    public Quote(string statement, string byline, bool isRelevant = true) : base()
    {
        this.Statement = statement;
        this.Byline = byline;
        this.IsRelevant = isRelevant;
    }

    /// <summary>
    /// Creates a new instance of a Quote object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="statement"></param>
    /// <param name="byline"></param>
    public Quote(long contentId, string statement, string byline, bool isRelevant)
        : this(statement, byline, isRelevant)
    {
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of a Quote object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="statement"></param>
    /// <param name="byline"></param>
    public Quote(Content content, string statement, string byline, bool isRelevant)
         : this(content.Id, statement, byline, isRelevant)
    {
    }
    #endregion
}
