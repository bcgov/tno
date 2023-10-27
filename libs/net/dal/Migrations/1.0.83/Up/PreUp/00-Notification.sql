DO $$
BEGIN

-- Copy the notification templates into temp table so they can be moved to separate table.
CREATE TEMP TABLE _notification_template AS
SELECT
  "id" as "notification_id"
  , concat_ws('-', "name", "owner_id") as "name"
  , "description"
  , "settings"->>'subject' as "subject"
  , "template" as "body"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "updated_by"
FROM public."notification";

-- Extract the notifications so that they can be recreated
CREATE TEMP TABLE _notification AS
SELECT
  "id"
  , "name"
  , concat_ws('-', "name", "owner_id") as "template_name"
  , "description"
  , "filter" as "settings"
  , "settings" as "query"
  , "notification_type"
  , "require_alert" as "alert_on_index"
  , "resend"
  , "owner_id"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
FROM public."notification";

-- Delete the notifications so that the table changes will work.
DELETE FROM public."notification";

END $$;
