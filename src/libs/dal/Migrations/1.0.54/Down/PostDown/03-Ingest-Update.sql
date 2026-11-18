DO $$

BEGIN

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - AudioVideo Content',
    description = 'TNO 1.0 - AudioVideo Content'
    where name = 'TNO 1.0 - AudioVideo Content - RECENT';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Print Content',
    description = 'TNO 1.0 - Print Content'
    where name = 'TNO 1.0 - Print Content - RECENT';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Image Content',
    description = 'TNO 1.0 - Image Content'
    where name = 'TNO 1.0 - Image Content - RECENT';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Story Content',
    description = 'TNO 1.0 - Story Content'
    where name = 'TNO 1.0 - Story Content - RECENT';

END $$;
