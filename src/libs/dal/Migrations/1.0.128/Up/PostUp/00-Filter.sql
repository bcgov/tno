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
    "id",
    "publishedOn",
    "topics.name",
    "topics.topicType"
  ]
}' -- query
WHERE "name" = 'Event of the Day - Topic Type - last 28 days'
AND owner_id = ServiceAccountId;

-- update 365 aggregate day filter
UPDATE public."filter"
SET "query" = '{
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
}' -- query
WHERE "name" = 'Event of the Day - Rolling 365 day aggregate'
AND owner_id = ServiceAccountId;

END $$;
