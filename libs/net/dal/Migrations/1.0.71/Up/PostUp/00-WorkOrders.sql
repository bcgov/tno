DO $$
BEGIN

UPDATE public."work_order"
SET "content_id" = (wo."configuration" -> 'contentId')::int
FROM public."work_order" wo
JOIN public."content" c ON (wo."configuration" -> 'contentId')::int = c."id"
WHERE wo."content_id" IS NULL
  AND public."work_order"."id" = wo."id";

END $$;
