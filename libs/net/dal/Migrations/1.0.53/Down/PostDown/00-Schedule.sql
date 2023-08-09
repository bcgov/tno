DO $$

BEGIN

delete from public.schedule
where id in
(select s.schedule_id from public.ingest i join public.ingest_schedule s on i.id = s.ingest_id
  where i.source_connection_id = (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database')
  and i.source_id = (SELECT id FROM public.source WHERE "name" = 'TNO 1.0')
  and i.name in (
  'TNO 1.0 - AudioVideo Content - HISTORIC', -- name
  'TNO 1.0 - Print Content - HISTORIC', -- name
  'TNO 1.0 - Image Content - HISTORIC', -- name
  'TNO 1.0 - Story Content - HISTORIC' -- name
)
);

END $$;
