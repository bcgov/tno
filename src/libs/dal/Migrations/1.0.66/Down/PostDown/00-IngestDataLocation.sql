DO $$

BEGIN

delete FROM public.ingest_data_location
WHERE ingest_id in (
    select id from public.ingest i
    where name in (
        'TNO 1.0 - AudioVideo Content - RECENTLY-PUBLISHED', -- name
        'TNO 1.0 - Print Content - RECENTLY-PUBLISHED', -- name
        'TNO 1.0 - Image Content - RECENTLY-PUBLISHED', -- name
        'TNO 1.0 - Story Content - RECENTLY-PUBLISHED' -- name
    )
);

END $$;
