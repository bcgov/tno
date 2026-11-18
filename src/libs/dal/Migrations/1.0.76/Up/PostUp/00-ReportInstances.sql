DO $$
BEGIN

UPDATE public."report_instance"
SET "status" = 2; -- Completed

END $$;
