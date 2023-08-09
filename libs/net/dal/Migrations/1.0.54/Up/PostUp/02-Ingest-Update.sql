DO $$

BEGIN

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - AudioVideo Content - RECENT',
    description = 'Ingest TNO 1.0 - AudioVideo Content - RECENT',
    configuration = '{ "post": true, "import": true, "importDateStart": "2023-08-01 12:00:00 am" }' -- configuration
    where name = 'TNO 1.0 - AudioVideo Content';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Print Content - RECENT',
    description = 'Ingest TNO 1.0 - Print Content - RECENT',
    configuration = '{ "post": true, "import": true, "importDateStart": "2023-08-01 12:00:00 am" }' -- configuration
    where name = 'TNO 1.0 - Print Content';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Image Content - RECENT',
    description = 'Ingest TNO 1.0 - Image Content - RECENT',
    configuration = '{ "post": true, "import": true, "importDateStart": "2023-08-01 12:00:00 am" }' -- configuration
    where name = 'TNO 1.0 - Image Content';

    UPDATE public.ingest
    set
    name = 'TNO 1.0 - Story Content - RECENT',
    description = 'Ingest TNO 1.0 - Story Content - RECENT',
    configuration = '{ "post": true, "import": true, "importDateStart": "2023-08-01 12:00:00 am" }' -- configuration
    where name = 'TNO 1.0 - Story Content';

END $$;
