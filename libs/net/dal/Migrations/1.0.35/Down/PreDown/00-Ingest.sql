DO $$
BEGIN

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Clips') --destination_connection_id
WHERE "name" = 'TNO 1.0 - AudioVideo Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Images') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Image Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Papers') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Print Content';

UPDATE public."ingest"
SET "destination_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Streams') --destination_connection_id
WHERE "name" = 'TNO 1.0 - Story Content';

END $$;
