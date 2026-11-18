DO $$
BEGIN

UPDATE public.ingest 
SET "topic" = 'TNO'
WHERE "topic" = 'TNO-HISTORIC' AND "name" LIKE '% - HISTORIC';

END $$;
