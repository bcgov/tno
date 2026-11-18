DO $$
BEGIN

INSERT INTO "notification_template" (
  "name"
  , "description"
  , "subject"
  , "body"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "updated_by"
)
SELECT
  "name"
  , "description"
  , COALESCE("subject", '')
  , "body"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "updated_by"
FROM _notification_template;

INSERT INTO "notification" (
  "id"
  , "name"
  , "description"
  , "settings"
  , "query"
  , "notification_type"
  , "notification_template_id"
  , "alert_on_index"
  , "resend"
  , "owner_id"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  "id"
  , "name"
  , "description"
  , "settings" - 'subject'
  , "query"
  , "notification_type"
  , (SELECT "id" FROM public."notification_template" WHERE "name" = n."template_name" LIMIT 1)
  , "alert_on_index"
  , "resend"
  , "owner_id"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
FROM _notification n;

DROP TABLE _notification_template;
DROP TABLE _notification;

END $$;
