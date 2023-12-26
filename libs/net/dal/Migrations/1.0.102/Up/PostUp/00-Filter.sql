DO $$
BEGIN

-- Create filters
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
),
(
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
),
(
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
;

END $$;
