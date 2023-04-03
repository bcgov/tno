DO $$
BEGIN

INSERT INTO public."notification" (
  "name"
  , "description"
  , "notification_type"
  , "require_alert"
  , "filter"
  , "resend"
  , "owner_id"
  , "is_public"
  , "settings"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Basic Alert' -- name
  , '' -- description
  , 0 -- notification_type
  , true -- require_alert
  , '{}' -- filter
  , 0 -- resend
  , 1 -- owner_id
  , true -- is_public
  , '{ "subject": "Testing" }' -- settings
  , 'Testing' -- template
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);

END $$;
