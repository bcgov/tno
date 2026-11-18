DO $$
DECLARE ServiceAccountId INT;
DECLARE FilterId INT;
DECLARE ChartTemplateId INT;
DECLARE ReportId INT;
DECLARE ReportTemplateId INT;
DECLARE ReportSectionId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

-- Create 28 day filter
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
  'Event of the Day - Topic Type - last 28 days' -- name
  , 'This filter is used as a source for the Event of the Day Report to show usage of Topic Types over a 28 day period' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , ServiceAccountId -- owner_id
  , '{
  "size": 1024,
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
              "gte": "now-28d/d",
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
  "size": 1024,
  "tags": [],
  "actions": [],
  "inStory": false,
  "hasTopic": true,
  "inByline": false,
  "sentiment": [],
  "seriesIds": [],
  "sourceIds": [],
  "dateOffset": -28,
  "inHeadline": false,
  "contentTypes": [],
  "mediaTypeIds": [],
  "contributorIds": [],
  "searchUnpublished": true
}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO FilterId;

-- Create 28 Day Chart Template
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
  'Topic Type - last 28 days' --name
  ,'Stacked line chart which shows usage of Topic Type over the last 28 days' --description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
 // Get all dates in data
  var dates = Content
    .GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue)
    .OrderBy(g=> g.Key)
    .ToArray();
  var topicTypeColorLookup = new Dictionary<string, string>
  {
    { "Issues", "#BB1111" },
    { "Proactive", "#006600" }
  };
  // Get all groups in data
  var groups = Content
    .GroupBy(
      c => SectionSettings.GroupBy switch {
        "topicType" => c.Topics.FirstOrDefault()?.TopicType.ToString(),
        _ => c.OtherSource
      },
      c => c,
      (k, c) => new { Label = k, Group = c.GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue) })
    .OrderBy(g=> g.Label)
    .ToArray();
}
{
  "labels": [@String.Join(",", dates.Select(c => $"\"{c.Key:dd-MMM}\""))],
  "datasets": [
    @for (var i = 0; i < groups.Length; i++)
    {
      @{
        var datasetColor = "";
        if (!topicTypeColorLookup.TryGetValue(groups[i].Label, out datasetColor )) {
          datasetColor = "";
        }
      }

      @($"{{ \"label\": \"{groups[i].Label}\", \"fill\": true, \"borderColor\": \"{@datasetColor}\",\"backgroundColor\": \"{@datasetColor}\", \"data\": [{@String.Join(",", dates.Select(d => groups[i].Group.Where(g => g.Key == d.Key).Count()))} ] }}")
      if (i < groups.Length-1) @(",")
    }
   ]
}' -- template
, '{
    "groupBy": [
        "topicType"
    ],
    "options": {
        "scales": {
            "x": {
                "ticks": {
                    "stepSize": 1
                }
            },
            "y": {
                "min": 0,
                "ticks": {
                    "stepSize": 0
                },
                "stacked": true
            }
        },
        "plugins": {
            "legend": {
                "title": {
                    "text": "Topic Type - last 28 days",
                    "display": true
                },
                "display": true
            },
            "datalabels": {
                "anchor": "center",
                "labels": {
                    "title": {
                        "display": true
                    }
                }
            }
        }
    },
    "chartTypes": [
        "line"
    ]
}' -- settings
, '' -- created_by
, '') -- updated_by
RETURNING id INTO ChartTemplateId;

-- get the 'Event of the Day' Report id and associated report template id
SELECT id, report_template_id INTO ReportId, ReportTemplateId
FROM public."report"
WHERE name = 'Event of the Day'
AND owner_id = ServiceAccountId;

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

-- Create report sections for 28 days filter.
INSERT INTO public."report_section" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "section_type"
  , "report_id"
  , "filter_id"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  '395f28be-4b7f-491b-a788-f1de86d09b7c' -- name
  , '' -- description
  , true -- is_enabled
  , 4 -- sort_order
  , 3 -- section_type
  , ReportId  -- report_id
  , FilterId -- filter_id
  , '{
    "label": "Topic Type - last 28 days",
    "sortBy": "",
    "groupBy": "",
    "direction": "row",
    "hideEmpty": false,
    "showImage": false,
    "showFullStory": false,
    "showHeadlines": false,
    "useAllContent": false,
    "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
) RETURNING id INTO ReportSectionId;

-- insert the new report section chart template reference
INSERT INTO public."report_section_chart_template" (
	"report_section_id",
  "chart_template_id",
  "settings",
  "sort_order",
  "created_by",
  "updated_by")
	VALUES (
    ReportSectionId -- report_section_id
    , ChartTemplateId -- chart_template_id
    , '{
    "title": "",
    "width": 400,
    "height": 300,
    "altText": "Image",
    "groupBy": "topicType",
    "options": {
        "scales": {
            "x": {
                "ticks": {
                    "stepSize": 1
                }
            },
            "y": {
                "min": 0,
                "ticks": {
                    "stepSize": 0
                },
                "stacked": true
            }
        },
        "plugins": {
            "legend": {
                "title": {
                    "text": "Topic Type - last 28 days",
                    "display": true
                },
                "display": true
            },
            "datalabels": {
                "anchor": "center",
                "labels": {
                    "title": {
                        "display": true
                    }
                }
            }
        }
    },
    "chartType": "line",
    "isHorizontal": false,
    "showDataLabels": false,
    "showLegendTitle": false
}'
    , 0 -- sort_order
    , '' -- created_by
    , '' -- updated_by
  );

END $$;
