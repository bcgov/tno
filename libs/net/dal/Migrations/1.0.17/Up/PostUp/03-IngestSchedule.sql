DO $$
BEGIN

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Snippet Content')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Snippet Content') -- schedule_id
  , ''
  , ''
),(
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Print Content')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Print Content') -- schedule_id
  , ''
  , ''
),(
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Image Content')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Image Content') -- schedule_id
  , ''
  , ''
),(
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Story Content')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Story Content') -- schedule_id
  , ''
  , ''
);

END $$;
