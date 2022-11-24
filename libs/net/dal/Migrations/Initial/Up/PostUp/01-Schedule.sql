DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
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
    , "created_by_id"
    , "created_by"
    , "created_on"
    , "updated_by_id"
    , "updated_by"
    , "updated_on"
    , "version"
) VALUES

-- ******************************************************
-- CBC News Video
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
),

-- ******************************************************
-- CBC Victoria Radio
-- ******************************************************
(
  'CBCV - 01' -- name
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
), (
  'CBCV - 02' -- name
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
),

-- ******************************************************
-- Continuous Schedules
-- ******************************************************
(
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
), (
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
), (
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
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
  , DEFAULT_USER_ID -- created_by_id
  , ''  -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , DEFAULT_USER_ID  -- updated_by
  , '' -- updated_on
  , CURRENT_TIMESTAMP -- version
  , '0'
);

END $$;
