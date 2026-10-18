-- Event of the Day only counts stories whose topics score above zero and are not the system
-- "Not Applicable" topic. Stories from sources without topic scoring score zero, so this also
-- leaves out topics Content-Analysis gave them before it was limited to scored sources.
DO $$
DECLARE ServiceAccountId INT;
DECLARE ZeroScore JSONB := '{"nested": {"path": "topics", "query": {"range": {"topics.score": {"lte": 0}}}}}';
DECLARE SystemTopic JSONB := '{"nested": {"path": "topics", "query": {"bool": {"must": [{"term": {"topics.isSystem": true}}]}}}}';
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" WHERE username = 'service-account';

-- The folder collector (used to re-collect a report), the 365 day aggregate and the 28 day chart.
UPDATE public."filter"
SET "query" = jsonb_set("query", '{query,bool,must_not}', COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) || jsonb_build_array(ZeroScore))
WHERE "name" IN ('Event of the Day - Folder Collector', 'Event of the Day - Rolling 365 day aggregate', 'Event of the Day - Topic Type - last 28 days')
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool') = 'object'
  AND NOT COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) @> jsonb_build_array(ZeroScore);

-- The charts already leave out "Not Applicable"; the folder collector must as well.
UPDATE public."filter"
SET "query" = jsonb_set("query", '{query,bool,must_not}', COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) || jsonb_build_array(SystemTopic))
WHERE "name" = 'Event of the Day - Folder Collector'
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool') = 'object'
  AND NOT COALESCE("query"->'query'->'bool'->'must_not', '[]'::jsonb) @> jsonb_build_array(SystemTopic);

-- The 24 hour topic score chart reads the folder's stories.
UPDATE public."chart_template"
SET "template" = REPLACE("template",
  'Content = Content.Where(x => x.Topics.All(a => !a.IsSystem));',
  'Content = Content.Where(x => x.Topics.Any() && x.Topics.All(a => !a.IsSystem && a.Score > 0));')
WHERE "name" = 'Topic Analysis - Topic Score';

END $$;
