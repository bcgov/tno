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
    [Column("SOURCE")]
    public string? Source { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("SUMMARY")]
    public string? Summary { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TYPE")]
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
    [Column("ARCHIVED_TO")]
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
    [Column("STRING1")]
    public string? string1 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING2")]
    public string? string2 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING3")]
    public string? string3 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING4")]
    public string? string4 { get; set; }

    /// <summary>
    /// get/set.
    /// Appears to map to [Columnist/Pundit]
    /// </summary>
    [Column("STRING5")]
    public string? string5 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING6")]
    public string? string6 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING7")]
    public string? string7 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING8")]
    public string? string8 { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STRING9")]
    public string? string9 { get; set; }

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
    [Column("FILENAME")]
    public string? FileName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FULLFILEPATH")]
    public string? FilePath { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("WEBPATH")]
    public string? WebPath { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("THISJUSTIN")]
    public bool ThisJustIn { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("IMPORTEDFROM")]
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
    [Column("TEXT")]
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
    [Column("CONTENTTYPE")]
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
    [Column("POSTEDBY")]
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
    [Column("TRANSCRIPT")]
    public string? Transcript { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_CATEGORY")]
    public string? EodCategory { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_CATEGORY_GROUP")]
    public string? EodGroup { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EOD_DATE")]
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
