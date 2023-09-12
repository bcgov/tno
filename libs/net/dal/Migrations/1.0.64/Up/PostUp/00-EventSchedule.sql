DO $$
BEGIN

DELETE FROM public."event_schedule";

DELETE FROM public."schedule"
WHERE "name" = 'Morning Report';


END $$;
