DO $$
BEGIN

UPDATE public."ingest_type"
SET "name" = 'TNO-AudioVideo'
WHERE "name" = 'TNO-Snippet';

END $$;
