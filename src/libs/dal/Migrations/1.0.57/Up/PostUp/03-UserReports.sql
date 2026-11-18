DO $$
BEGIN

-- Default any record as subscribed users.
UPDATE public."user_report"
SET
  "is_subscribed" = true;

END $$;
