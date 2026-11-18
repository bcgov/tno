DO $$
BEGIN

UPDATE public."filter"
SET "query" = REPLACE("query"::text,
    '{"term": {"topics.isSystem": true}}',
    '{"match": {"topics.name": "Not Applicable"}}')::jsonb
WHERE "query"::text LIKE '%{"term": {"topics.isSystem": true}}%';

UPDATE public."chart_template"
SET "template" = REPLACE(REPLACE("template",
    'x.Topics.All(a => !a.IsSystem)',
    'x.Topics.All(a => a.Name != "Not Applicable")'),
    'Content.Where(x => x.Topics.All(a => !a.IsSystem))',
    'Content.Where(x => x.Topics.All(a => a.Name != "Not Applicable"))')
WHERE "template" LIKE '%IsSystem%';

END $$;
