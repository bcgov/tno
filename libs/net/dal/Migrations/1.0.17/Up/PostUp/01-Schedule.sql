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
(
  'TNO 1.0 - AudioVideo Content' -- name
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
),(
  'TNO 1.0 - Print Content' -- name
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
),(
  'TNO 1.0 - Image Content' -- name
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
),(
  'TNO 1.0 - Story Content' -- name
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
);

END $$;
