DO $$
BEGIN

DELETE FROM public."schedule"
WHERE "id" IN (SELECT "schedule_id" FROM public."event_schedule" WHERE "event_type" = 4);

DELETE FROM public."setting"
WHERE "name" IN ('ReportRetentionDays', 'NotificationRetentionDays');

END $$;
