DO $$
BEGIN

UPDATE public."schedule"
SET "name" = 'TNO 1.0 - AudioVideo Content'
WHERE "name" = 'TNO 1.0 - Snippet Content';

UPDATE public."ingest"
SET "name" = 'TNO 1.0 - AudioVideo Content'
WHERE "name" = 'TNO 1.0 - Snippet Content';

END $$;
