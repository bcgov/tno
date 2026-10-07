-- History retention settings (0 disables a purge) and the daily purge event schedule.
DO $$
DECLARE scheduleId INT;
BEGIN

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
)
VALUES (
  'ReportRetentionDays'
  , 'Days report history (report instances, evening overview instances, cached AI results) is kept. 0 disables the purge.'
  , '90'
  , true
  , 0
  , ''
  , ''
), (
  'NotificationRetentionDays'
  , 'Days notification history is kept. 0 disables the purge.'
  , '30'
  , true
  , 0
  , ''
  , ''
)
ON CONFLICT DO NOTHING;

IF NOT EXISTS (SELECT 1 FROM public."event_schedule" WHERE "event_type" = 4) THEN
  INSERT INTO public."schedule" (
    "name"
    , "description"
    , "is_enabled"
    , "delay_ms"
    , "start_at"
    , "run_on_week_days"
    , "run_on_months"
    , "day_of_month"
    , "created_by"
    , "updated_by"
  )
  VALUES (
    'Purge History'
    , 'Daily purge of report and notification history.'
    , true
    , 0
    , '02:00:00'
    , 127
    , 0
    , 0
    , ''
    , ''
  )
  RETURNING "id" INTO scheduleId;

  INSERT INTO public."event_schedule" (
    "name"
    , "description"
    , "is_enabled"
    , "schedule_id"
    , "event_type"
    , "settings"
    , "created_by"
    , "updated_by"
  )
  VALUES (
    'Purge History'
    , 'Purges report history older than ReportRetentionDays and notification history older than NotificationRetentionDays.'
    , true
    , scheduleId
    , 4
    , '{}'
    , ''
    , ''
  );
END IF;

END $$;
