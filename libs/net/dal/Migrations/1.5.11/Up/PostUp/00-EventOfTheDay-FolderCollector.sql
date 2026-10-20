-- The Event of the Day folder collects every story with a topic, including the system
-- "Not Applicable" topic and stories scored zero, so editors can review them before the report
-- is sent. 1.5.10 left those stories out of the folder collector. The 365 day and 28 day filters
-- and the 24 hour topic score chart still leave them out.
DO $$
DECLARE ServiceAccountId INT;
DECLARE ZeroScore JSONB := '{"nested": {"path": "topics", "query": {"range": {"topics.score": {"lte": 0}}}}}';
DECLARE SystemTopic JSONB := '{"nested": {"path": "topics", "query": {"bool": {"must": [{"term": {"topics.isSystem": true}}]}}}}';
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" WHERE username = 'service-account';

UPDATE public."filter"
SET "query" = jsonb_set("query", '{query,bool,must_not}', COALESCE((
    SELECT jsonb_agg(clause)
    FROM jsonb_array_elements("query"->'query'->'bool'->'must_not') AS clause
    WHERE clause <> ZeroScore AND clause <> SystemTopic
  ), '[]'::jsonb))
WHERE "name" = 'Event of the Day - Folder Collector'
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool'->'must_not') = 'array';

END $$;
