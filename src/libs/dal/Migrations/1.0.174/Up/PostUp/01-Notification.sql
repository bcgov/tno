DO $$
BEGIN

INSERT INTO public."notification" (
  "name"
  , "description"
  , "notification_type"
  , "notification_template_id"
  , "alert_on_index"
  , "resend"
  , "owner_id"
  , "settings"
  , "query"
  , "is_public"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
)
SELECT
  'Transcript Approved' -- name
  , 'This notification is sent with an approved transcript.' -- description
  , 0 -- notification_type
  , (SELECT "id" FROM public."notification_template" WHERE "name" = 'Transcript Approved') -- notification_template_id
  , false -- alert_on_index
  , 0 -- resend
  , null -- owner_id
  , '{"size": 0, "searchUnpublished": false}' -- settings
  , '{}' -- query
  , false -- is_public
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
WHERE NOT EXISTS (
  SELECT "id" FROM public."notification" WHERE "name" = 'Transcript Approved'
);

END $$;
