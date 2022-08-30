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
);

END $$;
