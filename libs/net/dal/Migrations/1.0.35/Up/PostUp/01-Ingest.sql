DO $$
BEGIN

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Migrated AudioVideo') --destination_connection_id
WHERE "name" = 'TNO 1.0 - AudioVideo Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Migrated Images') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Image Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Migrated Papers') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Print Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Migrated Stories') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Story Content';

END $$;
