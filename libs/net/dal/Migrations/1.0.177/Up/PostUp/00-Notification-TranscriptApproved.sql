DO $$
BEGIN

UPDATE public."notification"
SET
  "alert_on_index" = true
  , "resend" = 3
WHERE "name" = 'Transcript Approved';

END $$;
