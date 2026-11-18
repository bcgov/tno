DO $$
BEGIN

-- Default any record as subscribed users.
UPDATE public."user_notification"
SET
  "is_subscribed" = true;

END $$;
