DO $$
BEGIN

INSERT INTO public.schedule (
    "name"
    , "description"
    , "is_enabled"
    , "schedule_type"
    , "delay_ms"
    , "run_on"
    , "start_at"
    , "stop_at"
    , "repeat"
    , "run_on_week_days"
    , "run_on_months"
    , "day_of_month"
    , "created_by"
    , "updated_by"
    , "version"
) VALUES

-- ******************************************************
-- Video
-- ******************************************************
(
  'CBC News - 01' -- name
  , '' -- description
  , true -- is_enabled
  , 3 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '07:00:00' -- start_at
  , '07:15:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
),
(
  'CBC News - 02' -- name
  , '' -- description
  , true -- is_enabled
  , 3 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '07:12:00' -- start_at
  , '07:20:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Raspberry Pi4' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '02:00:00' -- start_at
  , '22:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Raspberry Pi5' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '02:00:00' -- start_at
  , '22:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
)


-- ******************************************************
-- Radio
-- ******************************************************
, (
  'CBCV - Stream' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBCV - Clips' -- name
  , '' -- description
  , true -- is_enabled
  , 3 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '07:00:00' -- start_at
  , '07:15:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Kamloops' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Kelowna' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Prince George' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Vancouver' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Victoria' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CHKG' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CHMB' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CHNL' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CJCN' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CJVB' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKFU' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKSP' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKWX' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKYE' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKNW' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CFAX' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'KNKX' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CKFR' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC R2' -- name
  , '' -- description
  , true -- is_enabled
  , 2 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , '16:00:00' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
)

-- ******************************************************
-- Syndication
-- ******************************************************
, (
  'Castanet' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 60000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Canadian Press Wire' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 60000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Daily Hive' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'The Georgia Straight' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'iPolitics' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Business in Vancouver' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Prince George Citizen' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC Online' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Victoria Buzz' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Orca' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Narwhal' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'iNFOnews' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Ha-Shilth-Sa' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'The Tyee' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
)


-- ******************************************************
-- Front Pages
-- ******************************************************
, (
  'GLOBE' -- name
  , 'Globe and Mail front page images' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'POST' -- name
  , 'National Post front page images' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'PROVINCE' -- name
  , 'The Province front page images' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'TC' -- name
  , 'Times Colonist Victoria front page images' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'SUN' -- name
  , 'Vancouver Sun front page images' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
)

-- ******************************************************
-- Files
-- ******************************************************
, (
  'Blacks Newsgroup' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Meltwater' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'GLOBE - Articles' -- name
  , 'Globe and Mail article import' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'StarMetro' -- name
  , 'StarMetro article import' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 30000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
);

END $$;
