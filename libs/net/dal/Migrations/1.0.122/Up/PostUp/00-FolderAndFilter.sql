DO $$
DECLARE FolderId INT;
DECLARE ReportId INT;
BEGIN

-- get the 'Event of the Day' Folder id
SELECT Id INTO FolderId
FROM public."folder"
WHERE name = 'Event of the Day'
AND filter_id = (select id from public."filter" where name = 'Event of the Day - Folder Collector')
AND owner_id = (select id from public."user" where username = 'service-account');

-- get the 'Event of the Day' Report id
SELECT Id INTO ReportId
FROM public."report"
WHERE name = 'Event of the Day'
AND owner_id = (select id from public."user" where username = 'service-account');

-- Update the 'Event of the Day' Folder description
UPDATE public."folder" 
SET
    "description" = 'Folder used as source for Event of the Day feature'
WHERE id = FolderId;

-- Update Filter - Event of the Day - 24hr Aggregate
UPDATE public."filter" 
SET
    "description" = 'This filter returns the top X topics used over the last 24 hours. Will not include content set with topic "Not Applicable"',
    "query" = 
'{
  "aggs": {
    "aggTopics": {
      "aggs": {
        "aggTopicType": {
          "aggs": {
            "aggTopicName": {
              "terms": {
                "size": 100,
                "field": "topics.name",
                "order": [
                  {
                    "_count": "desc"
                  }
                ],
                "exclude": "Not Applicable"
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
  "size": 255,
  "sort": [
    {
      "publishedOn": "desc"
    }
  ]
}',
  "settings" =
'{
    "from": 0, 
    "size": 255, 
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
WHERE name = 'Event of the Day - 24hr Aggregate';

-- Update Filter - Event of the Day - Folder Collector
UPDATE public."filter" 
SET
    "description" = 'This filter is used as a source by a folder to collect content items which have a Topic set. 24hr lookback',
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
    "hasTopic": false, 
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
WHERE name = 'Event of the Day - Folder Collector';

-- delete any schedules associated with 'Event of the Day' folder
DELETE FROM public."event_schedule"
WHERE folder_id = FolderId;

-- Create invisble report sections with folder that will be cleared on report send.
INSERT INTO public."report_section" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "section_type"
  , "report_id"
  , "folder_id"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  '696adda3-f98a-40d0-8efe-90b614853fde' -- name
  , 'This section is included but not enabled because it forces the associated folder to be cleared when the report is sent out' -- description
  , false -- is_enabled
  , 3 -- sort_order
  , 0 -- section_type
  , ReportId  -- report_id
  , FolderId -- folderr_id
  , '{
    "label": "Hidden Section - DO NOT DELETE - See section summary for logic",
    "sortBy": "",
    "groupBy": "",
    "direction": "column",
    "hideEmpty": false,
    "showImage": false,
    "showFullStory": false,
    "showHeadlines": false,
    "useAllContent": false,
    "removeDuplicates": false
    }' -- settings
  , '' -- created_by
  , '' -- updated_by
);

-- update 'Event of the Day' report so that it clears the invisible folder when the report is emailed
UPDATE public."report"
SET settings = 
'{
    "content": {
        "clearFolders": true,
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
}'
WHERE id = ReportId;

END $$;
