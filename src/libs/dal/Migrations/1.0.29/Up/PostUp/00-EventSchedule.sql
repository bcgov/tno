DO $$
BEGIN

INSERT INTO public.schedule (
  "name"
  , "description"
  , "is_enabled"
  , "delay_ms"
  , "run_on"
  , "start_at"
  , "stop_at"
  , "repeat"
  , "run_on_week_days"
  , "run_on_months"
  , "day_of_month"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
) VALUES (
  'Morning Report' -- name
  , '' -- description
  , true -- is_enabled
  , 30000 -- delay_ms
  , NULL -- run_on
  , '06:00:00' -- start_at
  , NULL -- stop_at
  , false -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , '' -- updated_by
  , CURRENT_TIMESTAMP -- updated_on
);

INSERT INTO public."event_schedule" (
  "id"
  , "name"
  , "description"
  , "is_enabled"
  , "schedule_id"
  , "event_type"
  , "settings"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
) VALUES (
  1
  , 'Morning Report' -- name
  , '' -- description
  , true -- is_enabled
  , (SELECT id FROM public."schedule" WHERE "name" = 'Morning Report' ORDER BY "id" DESC LIMIT 1) -- schedule_id
  , 0 -- event_type
  , '{ "reportId": 2, "destination": 2 }'::jsonb -- settings
  , '' -- created_by
  , CURRENT_TIMESTAMP -- created_on
  , '' -- updated_by
  , CURRENT_TIMESTAMP -- updated_on
  );

END $$;
