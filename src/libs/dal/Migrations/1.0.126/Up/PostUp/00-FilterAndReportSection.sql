DO $$
DECLARE ServiceAccountId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

-- update 28 day filter
UPDATE public."filter"
SET "query" = '{
  "size": 1024,
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
        },
        {
          "nested": {
            "path": "topics",
            "query": {
              "bool": {
                "must": [
                  {
                    "match": {
                      "topics.name": "Not Applicable"
                    }
                  }
                ]
              }
            }
          }
        }
      ]
    }
  },
  "_source": [
    "publishedOn",
    "topics.name",
    "topics.topicType"
  ]
}' -- query
WHERE "name" = 'Event of the Day - Topic Type - last 28 days'
AND owner_id = ServiceAccountId;

-- update 28 Day Chart Template
UPDATE public."chart_template" 
SET "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
    // create a dictionary of dictionaries to flatten our data to something that can be accessed like this:
    // topicTypeAndCountByDate[date][topicType] 
    Dictionary<DateTime, Dictionary<string, int>> topicTypeAndCountByDate = Content
         .GroupBy(i => i.PublishedOn.HasValue ? new DateTime(i.PublishedOn!.Value.Year, i.PublishedOn!.Value.Month, i.PublishedOn!.Value.Day) : DateTime.MinValue)
         .ToDictionary(
             g => g.Key,
             g => g.GroupBy(gg => gg.Topics.Any() ? gg.Topics.FirstOrDefault()!.TopicType.ToString() : "N/A")
                 .ToDictionary(
                     ggg => ggg.Key,
                     ggg => ggg.ToList().Count()));

     // get a list of all the distinct dates
     DateTime[] dates = topicTypeAndCountByDate.Keys.OrderBy(x => x.ToUniversalTime()).ToArray();
     
     // create a dictionary to store a value per day for each topic type
     Dictionary<string, List<int>> topicTypeDataSets = new Dictionary<string, List<int>>
     {
         {"Issues", new List<int>()},
         {"Proactive", new List<int>()}
     };
     // for each date in the returned dataset
     foreach (var date in dates)
     {
         // iterate over the possible topic type hits on a day
         foreach(var topicType in topicTypeDataSets.Keys)
         {
             int topicTypeHits = 0;
             // get the target topic type hits for the day, or use ZERO if not found
             if (topicTypeAndCountByDate[date].ContainsKey(topicType))
             {
                 topicTypeHits = topicTypeAndCountByDate[date][topicType];
             }
             topicTypeDataSets[topicType].Add(topicTypeHits);
         };
     };
     var topicTypeColorLookup = new Dictionary<string, string>
     {
         { "Issues", "#BB1111" },
         { "Proactive", "#006600" }
     };
     int index = 0;
     int topicTypeCount = topicTypeDataSets.Keys.ToArray().Length;
}
{
  "labels": [@String.Join(",", dates.Select(x => $"\"{x:dd-MMM}\""))],
  "datasets": [
    @foreach (var topicType in topicTypeDataSets)
    {
      @{
        var datasetColor = "";
        if (!topicTypeColorLookup.TryGetValue(topicType.Key, out datasetColor )) {
          datasetColor = "";
        }
      }

      @($"{{ \"label\": \"{topicType.Key}\", \"fill\": true, \"borderColor\": \"{@datasetColor}\",\"backgroundColor\": \"{@datasetColor}\", \"data\": [{@String.Join(",",topicType.Value)} ] }}")
      if (++index < topicTypeCount) @(",")
    }
   ]
}' -- template
WHERE "name" = 'Topic Type - last 28 days';

-- Update last 365 days section.
UPDATE public."report_section" 
SET "settings" = '{
  "label": "Top Topics - Last 365 days",
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
WHERE "name" = 'fb499f42-6437-4fc2-9bd6-05918cf52539';

-- Update Filter - Event of the Day - Folder Collector
UPDATE public."filter" 
SET
    "description" = 'This filter is used as a source by a folder to collect content items which have a Topic set. 24hr lookback. Content must have a Topic Set',
    "query" = 
'{
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
}',
  "settings" =
'{
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
    "dateOffset": -1, 
    "inHeadline": false, 
    "contentTypes": [], 
    "mediaTypeIds": [], 
    "contributorIds": [], 
    "searchUnpublished": true
}'
WHERE "name" = 'Event of the Day - Folder Collector';

END $$;
