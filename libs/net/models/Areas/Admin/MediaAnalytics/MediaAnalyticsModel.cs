using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.MediaAnalytics;

/// <summary>
/// OrganizationModel class, provides a model that represents an organization.
/// </summary>
public class MediaAnalyticsModel:AuditColumnsModel
{

    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>

    public DateTime PublishedOn  { get; set; }

    /// <summary>
    /// get/set - The foreign key to the content.
    /// </summary>
    
    public int SourceId  { get; set; }


    /// <summary>
    /// get/set - The foreign key to the content.
    /// </summary>
    
    public int MediaTypeId  { get; set; }

    /// <summary>
    /// get/set - get the unique total count.
    /// </summary>
    
    public int UniqueViews  { get; set; }

    /// <summary>
    /// get/set - get the total count.
    /// </summary>
    
    public int TotalViews { get; set; }

    /// <summary>
    /// get/set - get the average views.
    /// </summary>
    
    public float AverageViews { get; set; }

    /// <summary>
    /// get/set - get the total male views
    /// </summary>
    
    public float MaleViewers { get; set; }

    /// <summary>
    /// get/set - get the total percentage of agegroup1
    /// </summary>
    
    public float AgeGroup1 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup1
    /// </summary>
   
    public string AgeGroup1Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of agegroup2.
    /// </summary>
    
    public float AgeGroup2 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup2.
    /// </summary>
    
    public string AgeGroup2Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of agegroup3.
    /// </summary>
    
    public float AgeGroup3 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup3.
    /// </summary>
    
    public string AgeGroup3Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of agegroup4.
    /// </summary>
    
    public float AgeGroup4 { get; set; }

    /// <summary>
    /// get/set - Label to describe agegroup4.
    /// </summary>
    
    public string AgeGroup4Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of pageview1 .
    /// </summary>
    
    public float PageViews1 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews1.
    /// </summary>
   
    public string PageViews1Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of pageview2.
    /// </summary>
    
    public float PageViews2 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews2.
    /// </summary>
    
    public string Page_Views2_Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of pageview3.
    /// </summary>
    
    public float PageViews3 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews3.
    /// </summary>
    
    public string Page_Views3_Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of pageview4.
    /// </summary>
   
    public float PageViews4 { get; set; }

    /// <summary>
    /// get/set - Label to describe PageViews4.
    /// </summary>
    
    public string Page_Views4_Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime1.
    /// </summary>
    
    public float WatchTime1 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime1.
    /// </summary>
   
    public string WatchTime1Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime2.
    /// </summary>
   
    public float WatchTime2 { get; set; }

    /// <summary>
    /// get/set -Label to describe WatchTime2
    /// </summary>
   
    public string WatchTime2Label { get; set; } = "";

    /// <summary>
    /// get/set - get the total percentage of WatchTime3.
    /// </summary>
    
    public float WatchTime3 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime3.
    /// </summary>
    
    public string WatchTime3Label { get; set; } = "";

     /// <summary>
    /// get/set - get the total percentage of WatchTime4.
    /// </summary>
    
    public float WatchTime4 { get; set; }

    /// <summary>
    /// get/set - Label to describe WatchTime4.
    /// </summary>
    
    public string WatchTime4Label { get; set; } = "";

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an OrganizationModel.
    /// </summary>
    public MediaAnalyticsModel()
    {
        
    }

    /// <summary>
    /// Creates a new instance of an MediaAnalyticsModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public MediaAnalyticsModel(Entities.MediaAnalytics entity,JsonSerializerOptions options) : base(entity)
    {
        this.PublishedOn = entity.PublishedOn;
        this.SourceId = entity.SourceId;
        this.MediaTypeId = entity.MediaTypeId;
        this.UniqueViews = entity.UniqueViews;
        this.TotalViews = entity.TotalViews;
        this.AverageViews = entity.AverageViews;
        this.MaleViewers = entity.MaleViewers;

        this.AgeGroup1 = entity.AgeGroup1;
        this.AgeGroup1Label = entity.AgeGroup1Label;
        this.AgeGroup2 = entity.AgeGroup2;
        this.AgeGroup1Label = entity.AgeGroup1Label;
        this.AgeGroup3 = entity.AgeGroup3;
        this.AgeGroup3Label = entity.AgeGroup3Label;
        this.AgeGroup4 = entity.AgeGroup4;
        this.AgeGroup4Label = entity.AgeGroup4Label;

        this.PageViews1 = entity.PageViews1;
        this.PageViews1Label = entity.PageViews1Label;
        this.PageViews2 = entity.PageViews2;
        this.Page_Views2_Label = entity.Page_Views2_Label;
        this.PageViews3 = entity.PageViews3;
        this.Page_Views3_Label = entity.Page_Views3_Label;
        this.PageViews4 = entity.PageViews4;
        this.Page_Views4_Label = entity.Page_Views4_Label;

        this.WatchTime1 = entity.WatchTime1;
        this.WatchTime1Label = entity.WatchTime1Label;
        this.WatchTime2 = entity.WatchTime2;
        this.WatchTime2Label = entity.WatchTime2Label;
        this.WatchTime3 = entity.WatchTime3;
        this.WatchTime3Label = entity.WatchTime3Label;
        this.WatchTime4 = entity.WatchTime4;
        this.WatchTime4Label = entity.WatchTime4Label;
    }

public static explicit operator Entities.MediaAnalytics(MediaAnalyticsModel model)
{
    var entity = new Entities.MediaAnalytics()
    {
        PublishedOn = model.PublishedOn,
        SourceId = model.SourceId,
        MediaTypeId = model.MediaTypeId,
        UniqueViews = model.UniqueViews,
        TotalViews = model.TotalViews,
        AverageViews = model.AverageViews,
        MaleViewers = model.MaleViewers,

        AgeGroup1 = model.AgeGroup1,
        AgeGroup1Label = model.AgeGroup1Label ?? string.Empty,
        AgeGroup2 = model.AgeGroup2,
        AgeGroup2Label = model.AgeGroup2Label ?? string.Empty,
        AgeGroup3 = model.AgeGroup3,
        AgeGroup3Label = model.AgeGroup3Label ?? string.Empty,
        AgeGroup4 = model.AgeGroup4,
        AgeGroup4Label = model.AgeGroup4Label ?? string.Empty,

        PageViews1 = model.PageViews1,
        PageViews1Label = model.PageViews1Label ?? string.Empty,
        PageViews2 = model.PageViews2,
        Page_Views2_Label = model.Page_Views2_Label ?? string.Empty,
        PageViews3 = model.PageViews3,
        Page_Views3_Label = model.Page_Views3_Label ?? string.Empty,
        PageViews4 = model.PageViews4,
        Page_Views4_Label = model.Page_Views4_Label ?? string.Empty,

        WatchTime1 = model.WatchTime1,
        WatchTime1Label = model.WatchTime1Label ?? string.Empty,
        WatchTime2 = model.WatchTime2,
        WatchTime2Label = model.WatchTime2Label ?? string.Empty,
        WatchTime3 = model.WatchTime3,
        WatchTime3Label = model.WatchTime3Label ?? string.Empty,
        WatchTime4 = model.WatchTime4,
        WatchTime4Label = model.WatchTime4Label ?? string.Empty
    };

    return entity;
}
   
    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentReference object.
    /// </summary>
    /// <returns></returns>
    public Entities.MediaAnalytics ToEntity()
    {
        var entity = (Entities.MediaAnalytics)this;
        return entity;
    }
    #endregion

    #endregion
}
