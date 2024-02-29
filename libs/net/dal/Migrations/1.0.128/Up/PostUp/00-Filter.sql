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

END $$;
