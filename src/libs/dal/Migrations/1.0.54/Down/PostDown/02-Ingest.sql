DO $$

BEGIN

delete from public.ingest
where source_connection_id = (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database')
  and source_id = (SELECT id FROM public.source WHERE "name" = 'TNO 1.0')
  and name in (
  'TNO 1.0 - AudioVideo Content - HISTORIC', -- name
  'TNO 1.0 - Print Content - HISTORIC', -- name
  'TNO 1.0 - Image Content - HISTORIC', -- name
  'TNO 1.0 - Story Content - HISTORIC' -- name
);

END $$;
