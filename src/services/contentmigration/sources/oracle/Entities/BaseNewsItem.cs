using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Services.ContentMigration.Sources.Oracle;
/// <summary>
/// NewsItem class, provides an entity to store News Item records in the database.
/// </summary>
public abstract class BaseNewsItem
{
    /// <summary>
    /// get/set.
    /// </summary>
    [Column("RSN")]
    public long RSN { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ITEM_DATE")]
    public DateTime? ItemDate { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ITEM_TIME")]
    public DateTime? ItemTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("SOURCE", TypeName = "VARCHAR2")]
    public string? Source { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("SUMMARY", TypeName = "VARCHAR2")]
    public string? Summary { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TITLE", TypeName = "VARCHAR2")]
    public string? Title { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TYPE", TypeName = "VARCHAR2")]
    public string Type { get; set; } = "";

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FRONTPAGESTORY")]
    public bool FrontPageStory { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("PUBLISHED")]
    public bool Published { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ARCHIVED")]
    public bool? Archived { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ARCHIVED_TO", TypeName = "VARCHAR2")]
    public string? ArchivedTo { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("RECORD_CREATED")]
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("RECORD_MODIFIED")]
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING1", TypeName = "VARCHAR2")]
    public string? String1 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING2", TypeName = "VARCHAR2")]
    public string? String2 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING3", TypeName = "VARCHAR2")]
    public string? String3 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING4", TypeName = "VARCHAR2")]
    public string? String4 { get; set; }

    /// <summary>
    /// get/set.
    /// Appears to map to [Columnist/Pundit]
    /// </summary>
    [Column("STRING5", TypeName = "VARCHAR2")]
    public string? String5 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING6", TypeName = "VARCHAR2")]
    public string? String6 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING7", TypeName = "VARCHAR2")]
    public string? String7 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING8", TypeName = "VARCHAR2")]
    public string? String8 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING9", TypeName = "VARCHAR2")]
    public string? String9 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("NUMBER1")]
    public long? Number1 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("NUMBER2")]
    public long? Number2 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("DATE1")]
    public DateTime? Date1 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("DATE2")]
    public DateTime? Date2 { get; set; }

    /// <summary>
    /// get/set.
    /// appears to be only a few file extensions used [.mp4, .m4a, .jpg, .pdf, .mov]
    /// </summary>
    [Column("FILENAME", TypeName = "VARCHAR2")]
    public string? FileName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FULLFILEPATH", TypeName = "VARCHAR2")]
    public string? FilePath { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("WEBPATH", TypeName = "VARCHAR2")]
    public string? WebPath { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("THISJUSTIN")]
    public bool ThisJustIn { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("IMPORTEDFROM", TypeName = "VARCHAR2")]
    public string? ImportedFrom { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EXPIRE_RULE")]
    public long ExpireRule { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("COMMENTARY")]
    public bool Commentary { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TEXT", TypeName = "VARCHAR2")]
    public string? Text { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("BINARY")]
    public byte[]? Binary { get; set; }

    /// <summary>
    /// get/set.
    /// Can be NULL or one of [video/quicktime, image/jpeg, application/pdf]
    /// </summary>
    [Column("CONTENTTYPE", TypeName = "VARCHAR2")]
    public string? ContentType { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("BINARYLOADED")]
    public bool BinaryLoaded { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("LOADBINARY")]
    public bool BinaryLoad { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EXTERNALBINARY")]
    public bool BinaryExternal { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CBRA_NONQSM")]
    public bool Cbra { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("POSTEDBY", TypeName = "VARCHAR2")]
    public string? PostedBy { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ONTICKER")]
    public bool OnTicker { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("WAPTOPSTORY")]
    public bool WapTopStory { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("ALERT")]
    public bool Alert { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("AUTO_TONE")]
    public long? AutoTone { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CATEGORIES_LOCKED")]
    public bool CategoriesLocked { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CORE_ALERT")]
    public bool CoreAlert { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("COMMENTARY_TIMEOUT")]
    public double? CommentaryTimeout { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("COMMENTARY_EXPIRE_TIME")]
    public long? CommentaryExpireTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TRANSCRIPT", TypeName = "VARCHAR2")]
    public string? Transcript { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_CATEGORY", TypeName = "VARCHAR2")]
    public string? EodCategory { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_CATEGORY_GROUP", TypeName = "VARCHAR2")]
    public string? EodGroup { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_DATE", TypeName = "VARCHAR2")]
    public string? EodDateTime { get; set; }

    /// <summary>
    /// get - Collection of tone values.
    /// </summary>
    public ICollection<UserTone> Tones { get; set; } = new List<UserTone>();

    /// <summary>
    /// get - Collection of EoD topics.
    /// </summary>
    public ICollection<NewsItemEoD> Topics { get; set; } = new List<NewsItemEoD>();

}
