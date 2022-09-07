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
) VALUES (
      'daily' --name
  , ''
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
      'morning' --name
  , ''
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
), (
      'default' --name
  , ''
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
);

END $$;