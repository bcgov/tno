DO $$
BEGIN

CREATE TEMP TABLE _work_order AS
SELECT "id", "content_id"
FROM public.work_order
WHERE "content_id" IS NOT NULL;

END $$;
