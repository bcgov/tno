DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.user (
  "username"
  , "key"
  , "email"
  , "display_name"
  , "email_verified"
  , "is_enabled"
  , "is_system_account"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'service-account' -- username
  , '616beebf-ce6c-4b28-bd5f-a32ceded524b' -- key
  , 'admin@local.com' -- email
  , 'Service Account' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , true -- isServiceAccount
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'admin' -- username
  , 'f3487a90-b793-4e60-b32c-2945591289aa' -- key
  , 'admin@local.com' -- email
  , 'Administrator' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , false -- isServiceAccount
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'editor' -- name
  , '1057697d-5cd0-4b00-97f3-df0f13849217' -- key
  , 'editor@local.com' -- email
  , 'Editor' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , false -- isServiceAccount
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'subscriber' -- name
  , 'e9b0ec48-c4c2-4c73-80a6-5c03da70261d' -- key
  , 'subscriber@local.com' -- email
  , 'Subscriber' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , false -- isServiceAccount
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
