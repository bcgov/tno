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
    WHERE clause <> ZeroScore AND ("name" <> 'Event of the Day - Folder Collector' OR clause <> SystemTopic)
  ), '[]'::jsonb))
WHERE "name" IN ('Event of the Day - Folder Collector', 'Event of the Day - Rolling 365 day aggregate', 'Event of the Day - Topic Type - last 28 days')
  AND "owner_id" = ServiceAccountId
  AND jsonb_typeof("query"->'query'->'bool'->'must_not') = 'array';

UPDATE public."chart_template"
SET "template" = REPLACE("template",
  'Content = Content.Where(x => x.Topics.Any() && x.Topics.All(a => !a.IsSystem && a.Score > 0));',
  'Content = Content.Where(x => x.Topics.All(a => !a.IsSystem));')
WHERE "name" = 'Topic Analysis - Topic Score';

END $$;
