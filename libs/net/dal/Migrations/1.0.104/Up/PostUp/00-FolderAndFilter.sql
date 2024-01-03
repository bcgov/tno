DO $$
DECLARE FolderFilterId INT;
BEGIN
-- Create Folder filter
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
  'Event of the Day - Folder Collector' -- name
  , 'This filter is used as a source by a folder to collect content items which have a Topic set' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , 1 -- owner_id
  , '{
  "size": 100,
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
  "dateOffset": -100,
  "inHeadline": false,
  "contentTypes": [],
  "mediaTypeIds": [],
  "contributorIds": [],
  "searchUnpublished": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
RETURNING id INTO FolderFilterId;

INSERT INTO public."folder" (
        "owner_id",
        "settings",
        "created_by",
        "updated_by",
        "name",
        "description",
        "is_enabled",
        "sort_order",
        "filter_id"
) VALUES (
        1 -- owner_id
        , '{
  "keepAgeLimit": 0
}' -- settings
    , '' -- created_by
    , '' -- updated_by
    , 'Event of the Day' -- name
    , '' -- description
    ,  true -- is_enabled
    ,  0 -- sort_order
    , FolderFilterId -- filter_id
    );

END $$;