DO $$
DECLARE ChartTemplateId INT;
DECLARE ReportTemplateId INT;
DECLARE ReportId INT;
DECLARE OneDayAggregateFilterId INT;
DECLARE OneYearAggregateFilterId INT;
BEGIN

-- Create Chart Template
INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "template"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Topic Analysis' --name
  ,'Pie chart that can be configured to use either Topic Type or Topic Name' --description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = SectionSettings.GroupBy switch
  {
    "topicType" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.TopicType.ToString() ?? "0").OrderBy(group => group.Key),
    "topicName" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.Name ?? "0").OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
}
{
  "labels": [@String.Join(",", group .Select(c => $"\"{c.Key}\""))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group .Select(c => string.Format("{0:F1}",  ((decimal)c.Count())/Content.Count() * 100)))],
      "backgroundColor": ["#BB1111", "#006600"]
    }
   ]
}' -- template
, '{
  "groupBy": [
    "topicType",
    "topicName"
  ],
  "options": {
    "plugins": {
      "legend": {
        "align": "center",
        "title": {
          "text": "Topic Analysis"
        },
        "labels": {
          "font": {
            "size": 18
          }
        },
        "display": true,
        "position": "bottom"
      },
      "datalabels": {
        "font": {
          "size": 18
        },
        "color": "#fff",
        "anchor": "center",
        "labels": {
          "font": {
            "size": 24
          },
          "title": {
            "display": true
          }
        },
        "formatter": "(value, ctx) => { const datapoints = ctx.chart.data.datasets[0].data; const total = datapoints.reduce((total, datapoint) => total + datapoint, 0); const percentage = value / total * 100; return percentage.toFixed(2) + ''%''; }"
      }
    }
  },
  "chartTypes": [
    "pie",
    "doughnut"
  ]
}' -- settings
, '' -- created_by
, '') -- updated_by
RETURNING id INTO ChartTemplateId;

-- Create report template
INSERT INTO public."report_template" (
  "name"
  , "description"
  , "report_type"
  , "is_enabled"
  , "sort_order"
  , "subject"
  , "body"
  , "settings"
  , "created_by"
  , "updated_by"
) values (
  'Event of the Day' -- name
  , 'Razor template for Event of the Day report.' -- description
  , 0 -- report_type
  , true -- is_enabled
  , 0 -- sort_order
  , '@using TNO.TemplateEngine
DEV - @Settings.Subject.Text @(Settings.Subject.ShowTodaysDate ? $" - {ReportExtensions.GetTodaysDate():dd-MMM-yyyy}" : "")' -- subject
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.ReportEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@using TNO.TemplateEngine
@{
  var pageBreak = Settings.Sections.UsePageBreaks ? "page-break-after: always;" : "";
  var utcOffset = ReportExtensions.GetUtcOffset(System.DateTime.Now, "Pacific Standard Time");
}
<h1 id="top" style="margin: 0; padding: 0;">@Settings.Subject.Text</h1>
<a name="top"></a>
<div style="color:red;">DO NOT FORWARD THIS EMAIL TO ANYONE</div>
<br/>

@if (Content.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@if (ViewOnWebOnly)
{
  <a href="@SubscriberAppUrl" target="_blank">Click to view this report online</a>
}
else
{
  var contentCount = 0;
  var allContent = Content.ToArray();
  foreach (var section in Sections)
  {
    var sectionContent = section.Value.Content.ToArray();

    if (!section.Value.IsEnabled) continue;
    if (sectionContent.Length == 0 && section.Value.Settings.HideEmpty) continue;

    <hr style="width: 100%; display: inline-block;" />
    <div style="@pageBreak">
      @if (!String.IsNullOrEmpty(section.Value.Settings.Label))
      {
        <h2 id="section-@section.Value.Id" styled="margin: 0; padding: 0;">@section.Value.Settings.Label<a name="section-@section.Value.Id"></a></h2>
      }
      @if (!String.IsNullOrEmpty(section.Value.Description))
      {
        <p>@section.Value.Description</p>
      }

      @if (section.Value.Settings.SectionType == ReportSectionType.TableOfContents)
      {
        @* Table of Contents Section *@
        var tocCount = 0;
        @foreach (var tableSection in Sections.Where(s => s.Value.Settings.SectionType != ReportSectionType.TableOfContents))
        {
          if (!tableSection.Value.Settings.HideEmpty || tableSection.Value.Content.Any())
          {
            <a href="#@($"section-{tableSection.Value.Id}")"><h3 style="margin: 0; padding: 0;">@tableSection.Value.Settings.Label</h3></a>
            @if (section.Value.Settings.ShowHeadlines && tableSection.Value.Content.Any())
            {
              <ul style="margin:0; margin-left: 25px; padding:0;">
                @foreach (var content in tableSection.Value.Content)
                {
                  <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{tocCount}")</li>
                  tocCount++;
                }
              </ul>
            }
          }
        }
      }
      else if (section.Value.Settings.SectionType == ReportSectionType.Text)
      {
        @* Text Section *@
      }
      else if (section.Value.Settings.SectionType == ReportSectionType.Content)
      {
        @* Content Section *@
        var positionInTOC = 0;
        if (section.Value.Settings.ShowHeadlines)
        {
            @if (section.Value.Settings.GroupBy != "")
            {
              var contentGroupings = sectionContent.GetContentGroupings(section.Value.Settings.GroupBy);
              <ul style="margin:0; margin-left: 25px; padding:0;">
              @foreach(KeyValuePair<string, List<long>> contentGroup in contentGroupings)
              {
              <li>@section.Value.Settings.GroupBy = @contentGroup.Key</li>
                <ul style="margin:0; margin-left: 25px; padding:0;">
                  @foreach(long contentId in contentGroup.Value)
                  {
                    var content = sectionContent.FirstOrDefault(c => c.Id == contentId);
                    var headlineLink = contentCount + positionInTOC;
                    positionInTOC++;
                    if (section.Value.Settings.ShowFullStory)
                    {
                      <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{headlineLink}")</li>
                    }
                    else
                    {
                      <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, "", "_blank")</li>
                    }
                  }
                </ul>
              }
              </ul>
            }
            else
            {
              <ul style="margin:0; margin-left: 25px; padding:0;">
              @foreach (var content in sectionContent)
              {
                var headlineLink= contentCount + positionInTOC;
                positionInTOC++;
                  @if (section.Value.Settings.ShowFullStory)
                  {
                    <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{headlineLink}")</li>
                  }
                  else
                  {
                    <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, "", "_blank")</li>
                  }
              }
              </ul>
            }
        }
        @* Show Aggregate detail if returned *@
        @if (section.Value.Settings.SectionType == ReportSectionType.Content)
        {
          if  (section.Value.Aggregations != null)
          {
            var showAggregateCountsAsPercentOfTotal = true;
            @foreach (var rootAggregation in section.Value.Aggregations)
            {
              var totalDocCount = rootAggregation.Value.DocCount;
              var rootAggregationChild = rootAggregation.Value.ChildAggregation;
              <ul>
              @foreach (var rootAggregationBucket in rootAggregationChild.Buckets)
              {
                var listItemColorStyle = ReportExtensions.GetColorFromName(rootAggregationBucket.Key, new string[] {"Issues","Proactive"}, new string[] {"#BB1111", "#006600"});
                if (showAggregateCountsAsPercentOfTotal)
                {
                  var rootAggregationAsPercentage = ((100*rootAggregationBucket.DocCount/totalDocCount));
                  <li style="font-weight:bold; @listItemColorStyle" >@rootAggregationBucket.Key (@rootAggregationAsPercentage %)</li>
                } else {
                  <li style="font-weight:bold; @listItemColorStyle" >@rootAggregationBucket.Key (@rootAggregationBucket.DocCount hits)</li>
                }
                if  (rootAggregationBucket.ChildAggregation != null) {
                  <ul>
                  @foreach (var childAggregationBucket in rootAggregationBucket.ChildAggregation.Buckets)
                  {
                    if (showAggregateCountsAsPercentOfTotal)
                    {
                      var childAggregationAsPercentage = ((100*childAggregationBucket.DocCount/totalDocCount));
                      <li style="@listItemColorStyle">@childAggregationBucket.Key (@childAggregationAsPercentage %)</li>
                    } else {
                      <li style="@listItemColorStyle">@childAggregationBucket.Key (@childAggregationBucket.DocCount)</li>
                      }
                   }
                   </ul>
                 }
               }
              </ul>
            }
          }
	    }

        @* Full Stories *@
        @if (section.Value.Settings.ShowFullStory)
        {
          for (var i = 0; i < sectionContent.Length; i++)
          {
            var content = sectionContent[i];
            var rawHeadline = ReportExtensions.GetHeadline(content, Model);
            var headline = Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawHeadline, "headline")  : rawHeadline;
            var rawBody = ReportExtensions.GetBody(content, Model);
            var body = Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawBody, "body")  : rawBody;
            var rawByline = ReportExtensions.GetByline(content, Model);
            var byline= Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawByline, "byline")  : rawByline;
            var hasPrev = contentCount > 0;
            var prev = hasPrev ? contentCount - 1 : 0;
            var hasNext = (contentCount  + 1) < allContent.Length;
            var next = hasNext ? contentCount + 1 : 0;
            var itemPosition = contentCount;
            contentCount++;
            <div>
              <div id="item-@itemPosition" style="display: flex; align-items: center;">
                <a name="item-@content.Id"></a>
              </div>
              <div>
                <a href="#top">top</a>
                @if (hasPrev )
                {
                  <a href="#item-@prev">previous</a>
                }
                @if (hasNext )
                {
                  <a href="#item-@next">next</a>
                }
              </div>
              <h3>@headline</h3>
              <div>@content.Source?.Name</div>
              <div>@content.PublishedOn?.AddHours(utcOffset).ToString("dddd, MMMM d, yyyy")</div>
              @if (!string.IsNullOrWhiteSpace(content.Page))
              {
                  <div>Page @content.Page</div>
              }
              @if (!string.IsNullOrWhiteSpace(byline) && Settings.Headline.ShowByline)
              {
                  <div>By @byline</div>
              }
              <br />
              @if (!String.IsNullOrEmpty(body))
              {
                <div>@body</div>
              }
              @if (!string.IsNullOrEmpty(content.ImageContent) && section.Value.Settings.ShowImage)
              {
                  var src = $"data:{content.ContentType};base64," + content.ImageContent;
                  <div><img src="@src" alt="@content.FileReferences.FirstOrDefault()?.FileName" /></div>
              }
              @if (Settings.Content.ShowLinkToStory)
              {
                <a href="@($"{ViewContentUrl}{content.Id}")" target="_blank">View Article</a>
              }
              <hr style="width: 100%; display: inline-block; border-top: 1px solid lightGrey;" />
            </div>
          }
        }
      }
      else if (section.Value.Settings.SectionType == ReportSectionType.MediaAnalytics)
      {
        @* Media Analytics Section *@
        foreach (var chart in section.Value.ChartTemplates)
        {
          <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
        }
      }
      else if (section.Value.Settings.SectionType == ReportSectionType.Gallery)
      {
        @* Gallery Section *@
        for (var i = 0; i < sectionContent.Length; i++)
        {
          var content = sectionContent[i];
          @if (!string.IsNullOrEmpty(content.ImageContent))
          {
              var src = $"data:{content.ContentType};base64," + content.ImageContent;
            if (section.Value.Settings.Direction == "row") {
              <img src="@src" alt="@content.FileReferences.FirstOrDefault()?.FileName" />
            } else {
              <div>
                <img src="@src" alt="@content.FileReferences.FirstOrDefault()?.FileName" />
              </div>
            }
          }
        }
      }
    </div>
  }
}

<p style="font-size: 9pt; margin-top: 0.5rem;">
  Terms of Use - This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.
  Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.
</p>' -- body
  , '{
  "Content": {
    "ExcludeReports": false,
    "ShowLinkToStory": false,
    "ExcludeHistorical": false,
    "HighlightKeywords": false
  },
  "Subject": {
    "Text": false,
    "ShowTodaysDate": false
  },
  "Headline": {
    "ShowSource": false,
    "ShowSentiment": false,
    "ShowShortName": false,
    "ShowPublishedOn": false
  },
  "Sections": {
    "Enable": false,
    "UsePageBreaks": false
  },
  "EnableCharts": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO ReportTemplateId;

INSERT INTO public."report_template_chart_template" (
	"report_template_id",
  "chart_template_id",
  "created_by",
  "updated_by")
	VALUES (
    ReportTemplateId -- report_template_id
    , ChartTemplateId -- chart_template_id
    , '' -- created_by
    , '' -- updated_by
  );

-- Insert new report
INSERT INTO public."report" (
  "name"
  , "description"
  , "owner_id"
  , "report_template_id"
  , "settings"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Event of the Day' -- name
  , '' -- description
  , 1 -- owner_id
  , ReportTemplateId -- report_template_id
  , '{
  "content": {
    "clearFolders": false,
    "excludeReports": [],
    "onlyNewContent": false,
    "showLinkToStory": false,
    "excludeHistorical": false,
    "highlightKeywords": false
  },
  "subject": {
    "text": "Event of the Day",
    "showTodaysDate": true
  },
  "headline": {
    "showByline": false,
    "showSource": false,
    "showSentiment": false,
    "showShortName": false,
    "showPublishedOn": false
  },
  "sections": {
    "usePageBreaks": false
  }
}' --settings
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO ReportId;

-- Create rolling 365 day aggregate filter
INSERT INTO public."filter" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "owner_id"
  , "query"
  , "settings"
  , "created_by"
  , "updated_by"
)
VALUES (
  'Event of the Day - Rolling 365 day aggregate' -- name
  , 'This filter returns the top X topics used over the last 365 days' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , 1 -- owner_id
  , '{
  "aggs": {
    "aggTopics": {
      "aggs": {
        "aggTopicType": {
          "aggs": {
            "aggTopicName": {
              "terms": {
                "size": 3,
                "field": "topics.name"
              }
            }
          },
          "terms": {
            "field": "topics.topicType"
          }
        }
      },
      "nested": {
        "path": "topics"
      }
    }
  },
  "size": 0,
  "query": {
    "bool": {
      "must": [
        {
          "range": {
            "publishedOn": {
              "gte": "now-365d/d",
              "time_zone": "US/Pacific"
            }
          }
        },
        {
          "nested": {
            "path": "topics",
            "query": {
              "exists": {
                "field": "topics"
              }
            }
          }
        }
      ]
    }
  }
}' -- query
  , '{
  "from": 0,
  "size": 100,
  "tags": [],
  "actions": [],
  "inStory": false,
  "hasTopic": true,
  "inByline": false,
  "sentiment": [],
  "seriesIds": [],
  "sourceIds": [],
  "dateOffset": -365,
  "inHeadline": false,
  "contentTypes": [],
  "mediaTypeIds": [],
  "contributorIds": [],
  "searchUnpublished": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO OneYearAggregateFilterId;

-- create 24hr aggregate filter
INSERT INTO public."filter" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "owner_id"
  , "query"
  , "settings"
  , "created_by"
  , "updated_by"
)
VALUES (
  'Event of the Day - 24hr Aggregate' -- name
  , 'This filter returns the top X topics used over the last 24 hours' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , 1 -- owner_id
  , '{
  "aggs": {
    "aggTopics": {
      "aggs": {
        "aggTopicType": {
          "aggs": {
            "aggTopicName": {
              "terms": {
                "size": 3,
                "field": "topics.name",
                "order": [
                  {
                    "_count": "desc"
                  }
                ]
              }
            }
          },
          "terms": {
            "field": "topics.topicType",
            "order": [
              {
                "_key": "desc"
              }
            ]
          }
        }
      },
      "nested": {
        "path": "topics"
      }
    }
  },
  "size": 1000,
  "sort": [
    {
      "publishedOn": "desc"
    }
  ],
  "query": {
    "bool": {
      "must": [
        {
          "range": {
            "publishedOn": {
              "gte": "now-1d/d",
              "time_zone": "US/Pacific"
            }
          }
        },
        {
          "nested": {
            "path": "topics",
            "query": {
              "exists": {
                "field": "topics"
              }
            }
          }
        }
      ],
      "must_not": [
        {
          "terms": {
            "mediaTypeId": [
              10
            ]
          }
        }
      ]
    }
  }
}' -- query
  , '{
  "from": 0,
  "size": 1000,
  "tags": [],
  "actions": [],
  "inStory": false,
  "hasTopic": true,
  "inByline": false,
  "sentiment": [],
  "seriesIds": [],
  "sourceIds": [],
  "dateOffset": -365,
  "inHeadline": false,
  "contentTypes": [],
  "mediaTypeIds": [],
  "contributorIds": [],
  "searchUnpublished": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO OneDayAggregateFilterId;

-- Create report sections.
INSERT INTO public."report_section" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "report_id"
  , "filter_id"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  '6ed85762-3634-4c6b-880d-77b88c3afac0' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , ReportId  -- report_id
  , OneDayAggregateFilterId -- filter_id
  , '{
  "label": "Top Topics - Last 24hrs",
  "sortBy": "",
  "groupBy": "",
  "direction": "",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "MediaAnalytics",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
),(
  'b5140713-c6d5-4be5-9318-94925876ca1c' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , ReportId  -- report_id
  , OneDayAggregateFilterId -- filter_id
  , '{
  "label": "",
  "sortBy": "",
  "groupBy": "",
  "direction": "column",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "Content",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
),
(
  'fb499f42-6437-4fc2-9bd6-05918cf52539' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , ReportId  -- report_id
  , OneYearAggregateFilterId -- filter_id
  , '{
  "label": "Top Topics - Last 356 days",
  "sortBy": "",
  "groupBy": "",
  "direction": "",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "Content",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
);

END $$;
