DO $$
BEGIN

UPDATE public."work_order" wo
SET "configuration" = wo."configuration" || jsonb_build_object('contentId', wo."content_id")
WHERE wo."content_id" IS NOT NULL;

END $$;
