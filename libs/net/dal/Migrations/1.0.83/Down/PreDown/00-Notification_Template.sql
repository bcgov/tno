DO $$
BEGIN

-- Copy the notification templates into temp table so they can be moved to separate table.
CREATE TEMP TABLE _notification_template AS
SELECT
  n."id" as "notification_id"
  , jsonb_set(n."settings", '{subject}', to_jsonb(nt."subject")) as "settings"
  , nt."body" as "template"
FROM public."notification" n
JOIN public."notification_template" nt on nt."id" = n."notification_template_id";

END $$;
