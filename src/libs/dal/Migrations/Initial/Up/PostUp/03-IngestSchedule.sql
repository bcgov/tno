DO $$
BEGIN

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'CBC Newsworld')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC News - 01') -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Newsworld')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC News - 02') -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria - Stream')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBCV - Stream') -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria - Clips')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBCV - Clips')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Castanet')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Castanet')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Canadian Press Wire')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Canadian Press Wire')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Blacks Newsgroup')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Blacks Newsgroup')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Meltwater')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Meltwater')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Globe & Mail - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'GLOBE')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Globe & Mail - Articles')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'GLOBE - Articles')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'The Province - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'PROVINCE')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Times Colonist Victoria - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'TC')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Vancouver Sun - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'SUN')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'National Post - Page 1')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'POST')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Kamloops')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Kamloops')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Kelowna')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Kelowna')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Prince George')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Prince George')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Vancouver')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Vancouver')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Victoria')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Victoria')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CHKG')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHKG')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CHMB')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHMB')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CHNL')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CHNL')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CJCN Connect FM')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CJCN')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CJVB')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CJVB')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKFU')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKFU')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKSP')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKSP')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKWX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKWX')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKYE')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKYE')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKNW')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKNW')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CFAX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CFAX')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'KNKX')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'KNKX')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CKFR')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CKFR')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC R2')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC R2')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Daily Hive')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Daily Hive')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'The Georgia Straight')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'The Georgia Straight')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'iPolitics')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'iPolitics')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Business in Vancouver')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Business in Vancouver')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Prince George Citizen')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Prince George Citizen')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC Online')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC Online')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Victoria Buzz')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Victoria Buzz')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Orca')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Orca')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Narwhal')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Narwhal')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'iNFOnews')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'iNFOnews')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Ha-Shilth-Sa')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Ha-Shilth-Sa')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'The Tyee')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'The Tyee')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'StarMetro')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'StarMetro')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Raspberry Pi4')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Raspberry Pi4')  -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Raspberry Pi5')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Raspberry Pi5')  -- schedule_id
  , ''
  , ''
);

END $$;
