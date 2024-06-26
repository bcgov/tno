using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Source.Entities;

[Table("NEWS_ITEMS")]
public class NewsItem
{
    [Key]
    [Column("RSN")]
    public long RSN { get; set; }

    [Column("ITEM_DATE")]
    public DateTime ItemDateTime { get; set; }

    [Column("ITEM_TIME")]
    public DateTime ItemTime { get; set; }

    [Column("SOURCE")]
    public string Source { get; set; } = "";

    [Column("SUMMARY")]
    public string? Summary { get; set; }

    [Column("TITLE")]
    public string? Title { get; set; }

    [Column("TYPE")]
    public string Type { get; set; } = "";

    [Column("FRONTPAGESTORY")]
    public bool FrontPageStory { get; set; }

    [Column("PUBLISHED")]
    public bool Published { get; set; }

    [Column("ARCHIVED")]
    public bool? Archived { get; set; }

    [Column("ARCHIVED_TO")]
    public string? ArchivedTo { get; set; }

    [Column("RECORD_CREATED")]
    public DateTime? CreatedOn { get; set; }

    [Column("RECORD_MODIFIED")]
    public DateTime? UpdatedOn { get; set; }

    [Column("STRING1")]
    public string? String1 { get; set; }

    [Column("STRING2")]
    public string? String2 { get; set; }

    [Column("STRING3")]
    public string? String3 { get; set; }

    [Column("STRING4")]
    public string? String4 { get; set; }

    [Column("STRING5")]
    public string? String5 { get; set; }

    [Column("STRING6")]
    public string? String6 { get; set; }

    [Column("STRING7")]
    public string? String7 { get; set; }

    [Column("STRING8")]
    public string? String8 { get; set; }

    [Column("STRING9")]
    public string? String9 { get; set; }

    [Column("NUMBER1")]
    public long? Number1 { get; set; }

    [Column("NUMBER2")]
    public long? Number2 { get; set; }

    [Column("DATE1")]
    public DateTime? Date1 { get; set; }

    [Column("DATE2")]
    public DateTime? Date2 { get; set; }

    [Column("FILENAME")]
    public string? FileName { get; set; }

    [Column("FULLFILEPATH")]
    public string? FilePath { get; set; }

    [Column("WEBPATH")]
    public string? WebPath { get; set; }

    [Column("THISJUSTIN")]
    public bool ThisJustIn { get; set; }

    [Column("IMPORTEDFROM")]
    public string? ImportedFrom { get; set; }

    [Column("EXPIRE_RULE")]
    public long ExpireRule { get; set; }

    [Column("COMMENTARY")]
    public bool Commentary { get; set; }

    [Column("TEXT")]
    public string? Text { get; set; }

    [Column("BINARY")]
    public byte[]? Binary { get; set; }

    [Column("CONTENTTYPE")]
    public string? ContentType { get; set; }

    [Column("BINARYLOADED")]
    public bool BinaryLoaded { get; set; }

    [Column("LOADBINARY")]
    public bool BinaryLoad { get; set; }

    [Column("EXTERNALBINARY")]
    public bool BinaryExternal { get; set; }

    [Column("CBRA_NONQSM")]
    public bool Cbra { get; set; }

    [Column("POSTEDBY")]
    public string? PostedBy { get; set; }

    [Column("ONTICKER")]
    public bool OnTicker { get; set; }

    [Column("WAPTOPSTORY")]
    public bool WapTopStory { get; set; }

    [Column("ALERT")]
    public bool Alert { get; set; }

    [Column("AUTO_TONE")]
    public long? AutoTone { get; set; }

    [Column("CATEGORIES_LOCKED")]
    public bool CategoriesLocked { get; set; }

    [Column("CORE_ALERT")]
    public bool CoreAlert { get; set; }

    [Column("COMMENTARY_TIMEOUT")]
    public double? CommentaryTimeout { get; set; }

    [Column("COMMENTARY_EXPIRE_TIME")]
    public long? CommentaryExpireTime { get; set; }

    [Column("TRANSCRIPT")]
    public string? Transcript { get; set; }

    [Column("EOD_CATEGORY")]
    public string? EodCategory { get; set; }

    [Column("EOD_CATEGORY_GROUP")]
    public string? EodGroup { get; set; }

    [Column("EOD_DATE")]
    public string? EodDateTime { get; set; }
}
