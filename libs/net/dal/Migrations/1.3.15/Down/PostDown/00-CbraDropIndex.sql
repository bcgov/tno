DO $$
BEGIN

DROP INDEX IF EXISTS public."IX_content_published_on";
DROP INDEX IF EXISTS public."IX_content_status";

END $$;
