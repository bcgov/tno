DO $$
BEGIN

UPDATE public."ingest" i
SET "configuration" = i."configuration" || jsonb_build_object('post', true) || jsonb_build_object('import', true)
WHERE "topic" = 'TNO';

UPDATE public."ingest" i
SET "configuration" = i."configuration" || jsonb_build_object('importMigrationType', 'Historic')
WHERE "name" like 'TNO 1.0 - % - HISTORIC';

UPDATE public."ingest" i
SET "configuration" = i."configuration" || jsonb_build_object('importMigrationType', 'Recent')
WHERE "name" like 'TNO 1.0 - % - RECENT';

UPDATE public."ingest" i
SET "configuration" = i."configuration" || jsonb_build_object('importMigrationType', 'RecentlyPublished')
WHERE "name" like 'TNO 1.0 - % - RECENTLY-PUBLISHED';

UPDATE public.ingest 
SET "topic" = 'TNO-HISTORIC'
WHERE "topic" = 'TNO' AND "name" LIKE '% - HISTORIC';

END $$;
