DO $$
DECLARE ServiceAccountId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

-- Update Filter - Event of the Day - 24hr Aggregate
UPDATE public."filter" 
SET "query" = 
'{
  "aggs": {
    "aggTopics": {
      "aggs": {
        "aggTopicType": {
          "aggs": {
            "agg_sum": {
              "sum": {
                "field": "topics.score"
              }
            },
            "aggTopicName": {
              "aggs": {
                "agg_sum": {
                  "sum": {
                    "field": "topics.score"
                  }
                },
                "agg_sum_sort": {
                    "bucket_sort": {
                        "sort": [
                            {
                            "agg_sum": {
                                "order": "desc"
                            }
                            }
                        ]
                    }
                }
              },
              "terms": {
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
  "size": 512,
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
  }
}',
"settings" =
'{
    "from": 0, 
    "size": 512, 
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
WHERE name = 'Event of the Day - 24hr Aggregate'
AND owner_id = ServiceAccountId;

END $$;
