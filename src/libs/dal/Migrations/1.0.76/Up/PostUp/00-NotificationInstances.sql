DO $$
BEGIN

UPDATE public."notification_instance"
SET "status" = 2; -- Completed

END $$;
