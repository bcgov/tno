DO $$
BEGIN

delete from public.ingest_type
where
  name = 'TNO-Snippet' -- 10
  and "description" = 'Snippet migration from TNO 1.0';

delete from public.ingest_type
where
  name = 'TNO-PrintContent' -- 11
  and "description" = 'Print Content migration from TNO 1.0';

delete from public.ingest_type
where
  name = 'TNO-Image' -- 10
  and "description" = 'Image migration from TNO 1.0';

delete from public.ingest_type
where
  name = 'TNO-Story' -- 10
  and "description" = 'Story migration from TNO 1.0';

END $$;
