DO $$
BEGIN

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - AudioVideo Content - RECENTLY-PUBLISHED')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - AudioVideo Content - RECENTLY-PUBLISHED') -- schedule_id
  , ''
  , ''
) ON CONFLICT DO NOTHING;

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Print Content - RECENTLY-PUBLISHED')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Print Content - RECENTLY-PUBLISHED') -- schedule_id
  , ''
  , ''
) ON CONFLICT DO NOTHING;

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Image Content - RECENTLY-PUBLISHED')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Image Content - RECENTLY-PUBLISHED') -- schedule_id
  , ''
  , ''
) ON CONFLICT DO NOTHING;

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'TNO 1.0 - Story Content - RECENTLY-PUBLISHED')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TNO 1.0 - Story Content - RECENTLY-PUBLISHED') -- schedule_id
  , ''
  , ''
);

END $$;
