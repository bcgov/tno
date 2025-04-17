using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
///MediaAnalytics class, provides a DB model to manage an evening overview instance section.
/// </summary>
[Table("MediaAnalytics")]
public class MediaAnalytics:BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
   
    [Column("published_On ")]
    public DateTime PublishedOn  { get; set; }

    /// <summary>
    /// get/set - The source of the story.
    /// </summary>
    public virtual Source? Source { get; set; }

    /// <summary>
    /// get/set - The foreign key to the content.
    /// </summary>
    [Column("source_id ")]
    public int SourceId  { get; set; }

     /// <summary>
    /// get/set - The media type this content will be placed in.
    /// </summary>
    public virtual MediaType? MediaType { get; set; }

    /// <summary>
    /// get/set - The foreign key to the content.
    /// </summary>
    [Column("mediaType_id ")]
    public int MediaTypeId  { get; set; }

    /// <summary>
    /// get/set - get the unique total count.
    /// </summary>
    [Column("unique_views")]
    public int UniqueViews  { get; set; }

    /// <summary>
    /// get/set - get the total count.
    /// </summary>
    [Column("total_views")]
    public int TotalViews { get; set; }

    /// <summary>
    /// get/set - get the average views.
    /// </summary>
    [Column("average_views")]
    public float AverageViews { get; set; }

    /// <summary>
    /// get/set - get the total male views
    /// </summary>
    [Column("male_viewers")]
    public float MaleViewers { get; set; }

    /// <summary>
    /// get/set - get the total percentage of agegroup1
    /// </summary>
    [Column("age_group1")]
    public float AgeGroup1 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup1
    /// </summary>
    [Column("age_group1_label")]
    public string AgeGroup1Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of agegroup2.
    /// </summary>
    [Column("age_group2")]
    public float AgeGroup2 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup2.
    /// </summary>
    [Column("age_group2_label")]
    public string AgeGroup2Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of agegroup3.
    /// </summary>
    [Column("age_group3")]
    public float AgeGroup3 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup3.
    /// </summary>
    [Column("age_group3_label")]
    public string AgeGroup3Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of agegroup4.
    /// </summary>
    [Column("age_group4")]
    public float AgeGroup4 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup4.
    /// </summary>
    [Column("age_group4_label")]
    public string AgeGroup4Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of pageview1 .
    /// </summary>
    [Column("page_views1")]
    public float PageViews1 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews1.
    /// </summary>
    [Column("page_views1_label")]
    public string PageViews1Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of pageview2.
    /// </summary>
    [Column("page_views2")]
    public float PageViews2 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews2.
    /// </summary>
    [Column("page_views2_label")]
    public string Page_Views2_Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of pageview3.
    /// </summary>
    [Column("page_views3")]
    public float PageViews3 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews3.
    /// </summary>
    [Column("page_views3_label")]
    public string Page_Views3_Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of pageview4.
    /// </summary>
    [Column("page_views4")]
    public float PageViews4 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews4.
    /// </summary>
    [Column("page_views4_label")]
    public string Page_Views4_Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime1.
    /// </summary>
    [Column("watch_time1")]
    public float WatchTime1 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime1.
    /// </summary>
    [Column("watch_time1_label")]
    public string WatchTime1Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime2.
    /// </summary>
    [Column("watch_time2")]
    public float WatchTime2 { get; set; }

    /// <summary>
    /// get/set -Label to describe WatchTime2
    /// </summary>
    [Column("watch_time2_label")]
    public string WatchTime2Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime3.
    /// </summary>
    [Column("watch_time3")]
    public float WatchTime3 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime3.
    /// </summary>
    [Column("watch_time3_label")]
    public string WatchTime3Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of WatchTime4.
    /// </summary>
    [Column("watch_time4")]
    public float WatchTime4 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime4.
    /// </summary>
    [Column("watch_time4_label")]
    public string WatchTime4Label { get; set; } = "";

    

}
#endregion