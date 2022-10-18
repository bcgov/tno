DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by_id"
  , "created_by"
  , "created_on"
  , "updated_by_id"
  , "updated_by"
  , "updated_on"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'CBC News')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC News - 01') -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC News')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC News - 02') -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria - Stream')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBCV - 01') -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria - Clips')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBCV - 02')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Castanet')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Castanet')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Canadian Press Wire')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Canadian Press Wire')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Blacks Newsgroup')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Blacks Newsgroup')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Meltwater')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Meltwater')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Globe & Mail - Front Pages')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'GLOBE')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Globe & Mail - Articles')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'GLOBE - Articles')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
);

END $$;
