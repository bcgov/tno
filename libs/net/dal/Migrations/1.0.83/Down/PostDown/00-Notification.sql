DO $$
BEGIN

UPDATE public."notification"
SET
  "settings" = nt."settings"
  , "template" = nt."template"
FROM _notification_template nt WHERE "notification"."id" = nt."notification_id";

DROP TABLE _notification_template;

END $$;
