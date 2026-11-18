DO $$
BEGIN

CREATE TEMP TABLE _work_order (
  "id"
  , "content_id"
) AS
SELECT "id"
  , ("configuration" ->> 'contentId')::bigint
FROM public.work_order;

END $$;
