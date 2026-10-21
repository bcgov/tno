-- Restore the 1.5.10 folder collector, which leaves out "Not Applicable" and stories scored zero.
DO $$
DECLARE ServiceAccountId INT;
DECLARE ZeroScore JSONB := '{"nested": {"path": "topics", "query": {"range": {"topics.score": {"lte": 0}}}}}';
DECLARE SystemTopic JSONB := '{"nested": {"path": "topics", "query": {"bool": {"must": [{"term": {"topics.isSystem": true}}]}}}}';
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" WHERE username = 'service-account';

UPDATE public."filter"
SET "query" = jsonb_set("query", '{query,bool,must_not}', COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) || jsonb_build_array(ZeroScore))
WHERE "name" = 'Event of the Day - Folder Collector'
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool') = 'object'
  AND NOT COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) @> jsonb_build_array(ZeroScore);

UPDATE public."filter"
SET "query" = jsonb_set("query", '{query,bool,must_not}', COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) || jsonb_build_array(SystemTopic))
WHERE "name" = 'Event of the Day - Folder Collector'
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool') = 'object'
  AND NOT COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) @> jsonb_build_array(SystemTopic);

END $$;
