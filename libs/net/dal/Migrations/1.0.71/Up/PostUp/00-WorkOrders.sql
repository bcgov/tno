DO $$
BEGIN

UPDATE public."work_order" wo
SET "content_id" = (wo."configuration" -> 'contentId')::int
WHERE wo."content_id" IS NULL;

END $$;
