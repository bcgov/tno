DO $$

BEGIN

DELETE FROM public.ingest_schedule
WHERE ingest_id IN (
    SELECT id FROM public.ingest WHERE TRIM(name) IN ('CBC | Aboriginal News', 'CBC | Top Stories', 'Link Newspaper')
);

DELETE FROM public.ingest WHERE TRIM(name) IN ('CBC | Aboriginal News', 'CBC | Top Stories', 'Link Newspaper');

DELETE FROM public.schedule WHERE TRIM(name) IN ('CBC | Aboriginal News', 'CBC | Top Stories News', 'Link Newspaper');

END $$;
