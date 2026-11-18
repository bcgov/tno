-- Topic scoring refactor.
DO $$
BEGIN

-- Renumber rules per source, preserving their current evaluation order.
UPDATE public."topic_score_rule" r
SET "sort_order" = o."new_order"
FROM (
  SELECT "id", ROW_NUMBER() OVER (PARTITION BY "source_id" ORDER BY "sort_order", "id") - 1 AS "new_order"
  FROM public."topic_score_rule"
) o
WHERE r."id" = o."id"
  AND r."sort_order" <> o."new_order";

-- The system topic is identified by its flag, not by its ID or name.
UPDATE public."topic"
SET "is_system" = true
WHERE "name" = 'Not Applicable';

-- No existing story's score changes until an editor resets it or a bulk rescore runs.
UPDATE public."content_topic"
SET "is_score_overridden" = true
WHERE "is_score_overridden" = false;

-- Event of the Day filters exclude the system topic by its flag.
UPDATE public."filter"
SET "query" = REPLACE("query"::text,
    '{"match": {"topics.name": "Not Applicable"}}',
    '{"term": {"topics.isSystem": true}}')::jsonb
WHERE "query"::text LIKE '%{"match": {"topics.name": "Not Applicable"}}%';

-- Chart templates exclude the system topic by its flag.
UPDATE public."chart_template"
SET "template" = REPLACE(REPLACE("template",
    'x.Topics.All(a => a.Name != "Not Applicable")',
    'x.Topics.All(a => !a.IsSystem)'),
    'Content.Where(x => x.Topics.All(a => a.Name != "Not Applicable"))',
    'Content.Where(x => x.Topics.All(a => !a.IsSystem))')
WHERE "template" LIKE '%"Not Applicable"%';

END $$;
