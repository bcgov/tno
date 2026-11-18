DO $$

BEGIN

delete FROM public.ingest_data_location
WHERE ingest_id in (
    select id from public.ingest i
    where name in (
        'TNO 1.0 - AudioVideo Content - HISTORIC', -- name
        'TNO 1.0 - Print Content - HISTORIC', -- name
        'TNO 1.0 - Image Content - HISTORIC', -- name
        'TNO 1.0 - Story Content - HISTORIC' -- name
    )
);

END $$;
