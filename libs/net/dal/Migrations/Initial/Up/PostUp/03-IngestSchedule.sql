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
  (SELECT id FROM public.ingest WHERE name = 'CBC Newsworld')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC News - 01') -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Newsworld')  -- ingest_id
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
  (SELECT id FROM public.ingest WHERE name = 'Globe & Mail - Page 1')  -- ingest_id
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
), (
  (SELECT id FROM public.ingest WHERE name = 'The Province - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'PROVINCE')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Times Colonist Victoria - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TC')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'Vancouver Sun - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'SUN')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'National Post - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'POST')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Kamloops')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Kamloops')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Kelowna')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Kelowna')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Prince George')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Prince George')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Vancouver')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Vancouver')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Victoria')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CHKG')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHKG')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CHMB')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHMB')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CHNL')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHNL')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CJCN Connect FM')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CJCN')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CJVB')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CJVB')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKFU')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKFU')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKSP')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKSP')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKWX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKWX')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKYE')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKYE')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKNW')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKNW')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CFAX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CFAX')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'KNKX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'KNKX')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CKFR')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKFR')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC R2')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC R2')  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
);

END $$;
